using desktop_app.Commands;
using desktop_app.Models;
using desktop_app.Services;
using desktop_app.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace desktop_app.ViewModels.Room
{
    /// <summary>
    /// ViewModel para crear una habitación.
    /// </summary>
    public class CreateRoomViewModel : ViewModelBase
    {
        /// <summary>Modelo de la habitación en creación.</summary>
        private RoomModel _room = new() { IsAvailable = true, Rate = 0 };
        public RoomModel Room
        {
            get => _room;
            set => SetProperty(ref _room, value);
        }

        private string _extrasText = "";
        /// <summary>Extras como texto CSV.</summary>
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
        /// <summary>Contador de imágenes extra.</summary>
        public string ExtraImagesLabel
        {
            get => _extraImagesLabel;
            set => SetProperty(ref _extraImagesLabel, value);
        }

        /// <summary>Comando para guardar.</summary>
        public AsyncRelayCommand SaveCommand { get; }

        /// <summary>Comando para cancelar.</summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>Comando para seleccionar imagen principal.</summary>
        public RelayCommand PickMainImageLocalCommand { get; }

        /// <summary>Comando para seleccionar imágenes extra.</summary>
        public RelayCommand PickExtraImagesLocalCommand { get; }

        public CreateRoomViewModel()
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(_ => NavigationService.Instance.NavigateTo<RoomView>());
            PickMainImageLocalCommand = new RelayCommand(_ => PickMainImageLocal());
            PickExtraImagesLocalCommand = new RelayCommand(_ => PickExtraImagesLocal());
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

            _extraImagesLocalPaths = dlg.FileNames.ToList();
            ExtraImagesLabel = $"{_extraImagesLocalPaths.Count} seleccionadas";
            ExtraImagesText = string.Join(", ", _extraImagesLocalPaths.Select(Path.GetFileName));
        }

        /// <summary>
        /// Valida, sube imágenes y crea la habitación.
        /// </summary>
        /// <exception cref="Exception">Si hay errores de validación o subida.</exception>
        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Room.RoomNumber))
                    throw new Exception("RoomNumber es obligatorio.");

                if (string.IsNullOrWhiteSpace(Room.Type))
                    throw new Exception("Type es obligatorio.");

                if (Room.MaxGuests <= 0)
                    throw new Exception("MaxGuests debe ser mayor que 0.");

                Room.Extras = (ExtrasText ?? "")
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (!string.IsNullOrWhiteSpace(_mainImageLocalPath))
                {
                    var uploadedMain = await ImageService.UploadSingleAsync(_mainImageLocalPath);
                    if (uploadedMain == null)
                        throw new Exception("No se pudo subir la imagen principal.");
                    Room.MainImage = uploadedMain.Url;
                }

                if (_extraImagesLocalPaths != null && _extraImagesLocalPaths.Count > 0)
                {
                    var uploadedExtras = await ImageService.UploadManyAsync(_extraImagesLocalPaths);
                    if (uploadedExtras == null)
                        throw new Exception("No se pudieron subir las imágenes extra.");
                    Room.ExtraImages = uploadedExtras.Select(x => x.Url).ToList();
                    ExtraImagesText = string.Join(", ", Room.ExtraImages);
                    ExtraImagesLabel = $"{Room.ExtraImages.Count} seleccionadas";
                }
                else
                {
                    Room.ExtraImages = new List<string>();
                    ExtraImagesText = "";
                    ExtraImagesLabel = "0 seleccionadas";
                }

                var created = await RoomService.CreateRoomAsync(Room);
                Reset(); // Limpia el formulario para la próxima vez
                NavigationService.Instance.NavigateTo<RoomView>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Limpia las propiedades del ViewModel para el siguiente uso.
        /// </summary>
        private void Reset()
        {
            Room = new RoomModel { IsAvailable = true, Rate = 0 };
            ExtrasText = "";
            ExtraImagesText = "";
            _mainImageLocalPath = null;
            _extraImagesLocalPaths.Clear();
            MainImageLabel = "Sin seleccionar";
            ExtraImagesLabel = "0 seleccionadas";
        }
    }
}
