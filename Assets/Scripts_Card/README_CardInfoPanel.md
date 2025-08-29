# Sistema de Panel Centralizado de Información de Cartas

## Descripción
Este sistema reemplaza el panel local de información de cartas con un panel centralizado que se muestra en el centro de la pantalla. El panel se activa con click derecho sobre una carta y se puede cerrar con click derecho nuevamente, Escape, o haciendo click en el botón de cerrar.

## Características
- **Panel centralizado**: Se muestra en el centro de la pantalla, no sobre la carta
- **Sin superposición**: Solo un panel puede estar activo a la vez
- **Bloqueo de interacciones**: El juego se pausa mientras el panel está activo
- **Múltiples formas de cerrar**: Click derecho, Escape, o botón de cerrar
- **Animaciones suaves**: Fade in/out del panel
- **Información personalizada**: Muestra sprite, nombre, descripción y estadísticas de cada carta

## Archivos del Sistema

### 1. CardInfoManager.cs
- **Función**: Gestiona la visualización del panel centralizado
- **Singleton**: Solo una instancia por escena
- **Métodos principales**:
  - `ShowCardInfo(CardData)`: Muestra la información de una carta
  - `HideCardInfo()`: Oculta el panel
  - `IsPanelActive()`: Verifica si el panel está activo

### 2. CardInfoPanelSetup.cs
- **Función**: Configuración básica del panel
- **Uso**: Script de configuración para el prefab del panel

### 3. CardInfoPanelPrefabSetup.cs
- **Función**: Configuración avanzada del prefab
- **Uso**: Asigna automáticamente las referencias al CardInfoManager

## Configuración en Unity

### Paso 1: Crear el Prefab del Panel
1. Crear un nuevo GameObject vacío en la escena
2. Agregar un Canvas como hijo (si no existe uno en la escena)
3. Configurar el Canvas para que sea "Screen Space - Overlay"
4. Agregar un CanvasGroup al GameObject raíz

### Paso 2: Estructura del Panel
```
CardInfoPanel (GameObject raíz)
├── Background (Image - botón invisible para cerrar)
└── MainPanel (GameObject con CanvasGroup)
    ├── CardIcon (Image)
    ├── TitleText (TMP_Text)
    ├── DescriptionText (TMP_Text)
    ├── StatsText (TMP_Text)
    └── CloseButton (Button)
```

### Paso 3: Configurar el CardInfoManager
1. Crear un GameObject vacío en la escena
2. Agregar el script `CardInfoManager`
3. Asignar las referencias del panel en el inspector

### Paso 4: Configurar el Prefab
1. Agregar el script `CardInfoPanelPrefabSetup` al GameObject raíz del panel
2. Asignar las referencias en el inspector
3. Convertir a prefab

## Uso del Sistema

### Para los Desarrolladores
El sistema se integra automáticamente con el `CardDisplay` existente. No se requieren cambios adicionales en el código de las cartas.

### Para los Usuarios
- **Click derecho sobre una carta**: Muestra la información de la carta
- **Click derecho nuevamente**: Cierra el panel
- **Tecla Escape**: Cierra el panel
- **Click en botón de cerrar**: Cierra el panel
- **Click fuera del panel**: Cierra el panel

## Personalización

### Colores del Panel
Los colores se pueden modificar en el inspector del `CardInfoPanelSetup`:
- `backgroundColor`: Color del fondo oscuro
- `panelColor`: Color del panel principal
- `textColor`: Color del texto normal
- `titleColor`: Color del título de la carta

### Animaciones
Las duraciones de las animaciones se pueden ajustar en el `CardInfoManager`:
- `fadeInDuration`: Duración del fade in (por defecto: 0.3s)
- `fadeOutDuration`: Duración del fade out (por defecto: 0.2s)

### Contenido del Panel
El panel muestra automáticamente:
- **Sprite de la carta**: Desde `CardData.icon`
- **Nombre de la carta**: Desde `CardData.cardName`
- **Descripción**: Desde `CardData.description`
- **Estadísticas**: Calculadas dinámicamente según el tipo de carta

## Ventajas del Nuevo Sistema

1. **Mejor UX**: El panel no obstaculiza la vista de la carta
2. **Consistencia**: Todas las cartas usan el mismo formato de visualización
3. **Escalabilidad**: Fácil de modificar y extender
4. **Performance**: No hay múltiples paneles activos simultáneamente
5. **Accesibilidad**: Múltiples formas de cerrar el panel

## Solución de Problemas

### El panel no se muestra
- Verificar que `CardInfoManager` esté en la escena
- Verificar que las referencias estén asignadas correctamente
- Verificar que el prefab del panel esté configurado

### El panel no se cierra
- Verificar que el `CanvasGroup` esté configurado correctamente
- Verificar que los botones de cerrar estén configurados
- Verificar que no haya conflictos con otros scripts

### Las estadísticas no se muestran correctamente
- Verificar que `GameManager.Instance` esté disponible
- Verificar que los multiplicadores estén configurados
- Verificar que `CardData` tenga los valores correctos
