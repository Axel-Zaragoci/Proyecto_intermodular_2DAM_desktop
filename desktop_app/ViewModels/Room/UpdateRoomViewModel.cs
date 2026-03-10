using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace desktop_app.ViewModels.Room
{
    /// <summary>
    /// ViewModel para editar una habitación.
    /// </summary>
    public class UpdateRoomViewModel : ViewModelBase
    {
        private static UpdateRoomViewModel? _instance;
        /// <summary>Instancia Singleton del ViewModel.</summary>
        public static UpdateRoomViewModel Instance =>
            _instance ??= new UpdateRoomViewModel();

        /// <summary>Item de imagen para la galería.</summary>
        public class RoomImageItem
        {
            /// <summary>Ruta relativa.</summary>
            public string Url { get; set; } = "";

            /// <summary>URL absoluta para mostrar.</summary>
            public string AbsoluteUrl { get; set; } = "";

            /// <summary>Es imagen principal.</summary>
            public bool IsMain { get; set; }
        }

        private RoomModel _room;
        /// <summary>Habitación en edición.</summary>
        public RoomModel Room
        {
            get => _room;
            set
            {
                SetProperty(ref _room, value);
                ExtrasText = string.Join(", ", value?.Extras ?? new());
                ExtraImagesText = string.Join(", ", value?.ExtraImages ?? new());
                RefreshExistingImages();
                _ = LoadReviewAverageAsync();
            }
        }

        private string _extrasText = "";
        /// <summary>Extras como CSV.</summary>
        public string ExtrasText
        {
            get => _extrasText;
            set => SetProperty(ref _extrasText, value);
        }

        private string _extraImagesText = "";
        /// <summary>Info de imágenes extra.</summary>
        public string ExtraImagesText
        {
            get => _extraImagesText;
            set => SetProperty(ref _extraImagesText, value);
        }

        /// <summary>Imágenes existentes para la galería.</summary>
        public ObservableCollection<RoomImageItem> ExistingImages { get; } = new();

        private string? _mainImageLocalPath;
        private List<string> _extraImagesLocalPaths = new();

        private string _mainImageLabel = "Sin seleccionar";
        /// <summary>Nombre del archivo de imagen principal.</summary>
        public string MainImageLabel
        {
            get => _mainImageLabel;
            set => SetProperty(ref _mainImageLabel, value);
        }

        private string _extraImagesLabel = "0 seleccionadas";
        /// <summary>Contador de imágenes extra nuevas.</summary>
        public string ExtraImagesLabel
        {
            get => _extraImagesLabel;
            set => SetProperty(ref _extraImagesLabel, value);
        }

        /// <summary>Texto formateado de la media de reviews.</summary>
        private string _reviewAverageText = "";
        public string ReviewAverageText
        {
            get => _reviewAverageText;
            set => SetProperty(ref _reviewAverageText, value);
        }

        /// <summary>Indica si hay reviews para mostrar la media.</summary>
        private bool _hasReviewAverage;
        public bool HasReviewAverage
        {
            get => _hasReviewAverage;
            set => SetProperty(ref _hasReviewAverage, value);
        }

        /// <summary>Comando para guardar cambios.</summary>
        public AsyncRelayCommand SaveCommand { get; }

        /// <summary>Comando para cancelar.</summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>Comando para seleccionar nueva imagen principal.</summary>
        public RelayCommand PickMainImageLocalCommand { get; }

        /// <summary>Comando para seleccionar nuevas imágenes extra.</summary>
        public RelayCommand PickExtraImagesLocalCommand { get; }

        /// <summary>Comando para eliminar una imagen.</summary>
        public AsyncRelayCommand<RoomImageItem> DeleteImageCommand { get; }

        /// <summary>
        /// Constructor privado para el patrón Singleton.
        /// </summary>
        private UpdateRoomViewModel()
        {
            _room = new RoomModel();

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(_ => NavigationService.Instance.NavigateTo<RoomView>());
            PickMainImageLocalCommand = new RelayCommand(_ => PickMainImageLocal());
            PickExtraImagesLocalCommand = new RelayCommand(_ => PickExtraImagesLocal());

            DeleteImageCommand = new AsyncRelayCommand<RoomImageItem>(
                DeleteImageAsync,
                (img) => img != null && !string.IsNullOrWhiteSpace(img.Url)
            );
        }

        /// <summary>
        /// Carga las reviews de la habitación y calcula la media.
        /// </summary>
        private async Task LoadReviewAverageAsync()
        {
            try
            {
                // Limpiar siempre el estado anterior al cambiar de habitación
                ReviewAverageText = "";
                HasReviewAverage = false;
                
                if (string.IsNullOrWhiteSpace(Room.Id)) return;

                var reviews = await ReviewService.GetReviewsByRoomAsync(Room.Id);
                if (reviews != null && reviews.Count > 0)
                {
                    var avg = reviews.Average(r => r.Rating);
                    ReviewAverageText = $" {avg:F1} / 5 ";
                    HasReviewAverage = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateRoomVM] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Recarga la galería de imágenes existentes.
        /// </summary>
        public void RefreshExistingImages()
        {
            ExistingImages.Clear();

            if (!string.IsNullOrWhiteSpace(Room.MainImage))
            {
                ExistingImages.Add(new RoomImageItem
                {
                    Url = Room.MainImage,
                    AbsoluteUrl = ImageService.ToAbsoluteUrl(Room.MainImage),
                    IsMain = true
                });
            }

            if (Room.ExtraImages != null)
            {
                foreach (var u in Room.ExtraImages.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    ExistingImages.Add(new RoomImageItem
                    {
                        Url = u,
                        AbsoluteUrl = ImageService.ToAbsoluteUrl(u),
                        IsMain = false
                    });
                }
            }

            OnPropertyChanged(nameof(ExistingImages));
        }

        /// <summary>
        /// Elimina una imagen del servidor y del modelo.
        /// </summary>
        /// <param name="img">RoomImageItem a eliminar.</param>
        private async Task DeleteImageAsync(RoomImageItem img)
        {
            if (img == null) return;

            var confirm = MessageBox.Show(
                "¿Seguro que quieres borrar esta imagen?\n\nSe eliminará del servidor y de la habitación.",
                "Confirmar borrado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            // 1. Borrar la imagen del servidor
            var ok = await ImageService.DeleteImageAsync(img.Url);
            if (!ok)
            {
                MessageBox.Show("No se pudo borrar la imagen en el servidor.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Actualizar el modelo local
            ExistingImages.Remove(img);

            if (img.IsMain)
            {
                Room.MainImage = null;
                MainImageLabel = "Sin seleccionar";
            }
            else
            {
                Room.ExtraImages?.Remove(img.Url);
                // Sanitize list to remove potential nulls/gaps
                if (Room.ExtraImages != null)
                {
                    Room.ExtraImages = Room.ExtraImages.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                }

                ExtraImagesLabel = $"{Room.ExtraImages?.Count ?? 0} seleccionadas";
                ExtraImagesText = string.Join(", ", Room.ExtraImages ?? new List<string>());
            }

            // 3. Guardar los cambios en la base de datos
            var updated = await RoomService.UpdateRoomAsync(Room.Id, Room);
            if (!updated)
            {
                MessageBox.Show("La imagen se borró del servidor, pero no se pudo actualizar la habitación en la base de datos.", 
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            OnPropertyChanged(nameof(Room));
        }

        private void PickMainImageLocal()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.webp;*.gif",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            _mainImageLocalPath = dlg.FileName;
            MainImageLabel = Path.GetFileName(_mainImageLocalPath);
        }

        private void PickExtraImagesLocal()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.webp;*.gif",
                Multiselect = true
            };

            if (dlg.ShowDialog() != true) return;

            // Acumular las nuevas imágenes seleccionadas en lugar de reemplazarlas
            var newFiles = dlg.FileNames.ToList();
            _extraImagesLocalPaths.AddRange(newFiles);
            
            ExtraImagesLabel = $"{_extraImagesLocalPaths.Count} seleccionadas";
        }

        /// <summary>
        /// Guarda los cambios en la habitación.
        /// </summary>
        /// <exception cref="Exception">Si hay errores de subida o actualización.</exception>
        private async Task SaveAsync()
        {
            try
            {
                Room.Extras = (ExtrasText ?? "")
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (!string.IsNullOrWhiteSpace(_mainImageLocalPath))
                {
                    // Si ya hay una imagen principal, borrarla del servidor antes de subir la nueva
                    if (!string.IsNullOrWhiteSpace(Room.MainImage))
                    {
                        await ImageService.DeleteImageAsync(Room.MainImage);
                    }
                    
                    var up = await ImageService.UploadSingleAsync(_mainImageLocalPath);
                    if (up == null) throw new Exception("No se pudo subir la imagen principal.");
                    Room.MainImage = up.Url;
                }

                if (_extraImagesLocalPaths.Count > 0)
                {
                    var ups = await ImageService.UploadManyAsync(_extraImagesLocalPaths);
                    if (ups == null) throw new Exception("No se pudieron subir las imágenes extra.");

                    Room.ExtraImages ??= new List<string>();
                    Room.ExtraImages.AddRange(ups.Select(x => x.Url));
                    ExtraImagesText = string.Join(", ", Room.ExtraImages);
                }

                var ok = await RoomService.UpdateRoomAsync(Room.Id, Room);
                if (!ok) throw new Exception("No se pudo actualizar la habitación (API).");

                NavigationService.Instance.NavigateTo<RoomView>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
