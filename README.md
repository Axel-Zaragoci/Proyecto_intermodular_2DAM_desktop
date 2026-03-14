# APLICACIÓN .NET PARA PROYECTO INTERMODULAR 2º DAM

## Contexto
Para mi curso de 2DAM, se nos ha propuesto el tema de crear unas aplicaciones para la gestión de un hotel. En cuanto a las aplicaciones, se debe crear una aplicación para escritorio utilizando el *framework* **.NET** para *C#* y *WPF* y una aplicación para móviles utilizando *Kotlin* y *Jetpack Compose*. Además, se ha de crear una API en *node.js* y *Express* que será consumida por ambas aplicaciones.

Este proyecto ha sido dividido en 2 partes. La primera parte, de modalidad grupal, es en la que se crea una primera versión con las funcionalidades básicas. 
[Aquí se puede acceder a la primera parte](https://github.com/Axel-Zaragoci/Proyecto_intermodular_2DAM)

Este repositorio corresponde a la API de la segunda parte, de modalidad individual.

## Objetivos
Los objetivos de esta segunda entrega se dividen en propuestas comunes y propuestas específicas. Las propuestas comunes son puntos a desarrollar que cada persona de la clase tiene que realizar, mientras que las especificas varían según el alumno.

Las propuestas comunes son el desarrollo de un sistema de auditorías de modificaiones de reservas y la creación de facturas descargables en formato PDF.

Para esta aplicación, en cuanto a la auditoría de reservas, se tienen que implementar los siguientes puntos:
- En la ventana de detalles de reserva, añadir una pestaña **"Historial"** con una línea de tiempo de todos los cambios
- Cada entrada debe de mostrar:
    - Acción
    - Actor
    - Fecha y hora
    - Cambios que se han realizado
- Implementar un filtro de historial por tipo de acción

Por otra parte, para las facturas descargables en PDF, se debe añadir lo siguiente:
- Agregar un botón **"Descargar factura"** en la ventana de detalles de reserva que abre o descarga el pdf
- Crear un módulo de facturas con:
    - Un listado de todas las facturas generadas
    - Filtros de facturas
    - Reenvío de factura por email
- Capacidad de configurar el encabezado de la factura (nombre del hotel, CIF y dirección)

Pasando a la parte específica, para el sistema de pagos se debe de implementar lo siguiente:
- En la ventana de detalles de la reserva, agregar una sección de pagos con:
    - Historial
    - Importe pendiente
    - Método usado
- Botón de *"Registrar pago"* con selector de método (efectivo, tarjeta, transferencia) e importe
- Agregar listado global de pagos con filtros por estado y método
- Exportar a CSV el listado de pagos

Finalmente, para los recordatorios se debe de crear las siguientes funcionalidades:
- En la pantalla de detalles de reserva, indicar las fechas y estados de los recordatorios enviados (enviado / pendiente / error)
- Agregar botón *"Reenviar recordatorio"* con selector de tipo (24/48 horas)
- Agregar panel de recordatorios del día, listando las reservas que recibirán recordatorio próximamente