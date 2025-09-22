# Configuración del Panel de Información de Cartas

## Descripción
El sistema de panel de información de cartas permite mostrar información detallada de una carta al hacer click derecho sobre ella. El panel bloquea todas las demás interacciones mientras está abierto.

## Archivos Creados/Modificados

### Nuevos Archivos:
- `CardInfoPanel.cs` - Script principal del panel
- `CARD_INFO_PANEL_SETUP.md` - Esta documentación

### Archivos Modificados:
- `GameManager.cs` - Agregado sistema de bloqueo de interacciones
- `CardDisplay.cs` - Modificado click derecho para abrir el panel
- `Card.cs` - Agregadas verificaciones de bloqueo de interacciones

## Configuración en Unity

### 1. Crear el Panel de Información

1. **Crear Canvas para el Panel:**
   - Crear un nuevo Canvas (UI > Canvas)
   - Nombrarlo "CardInfoCanvas"
   - Configurar como "Screen Space - Overlay"
   - Establecer Sort Order alto (ej: 100) para que aparezca encima de todo

2. **Crear el Panel Principal:**
   - Crear un GameObject vacío como hijo del Canvas
   - Nombrarlo "CardInfoPanel"
   - Agregar componente `Image` con color semi-transparente (ej: negro con alpha 0.8)
   - Agregar componente `CardInfoPanel` script

3. **Crear el Panel de Contenido:**
   - Crear un GameObject como hijo de "CardInfoPanel"
   - Nombrarlo "ContentPanel"
   - Agregar componente `Image` con color de fondo (ej: blanco/gris claro)
   - Configurar tamaño para ocupar gran parte de la pantalla
   - Agregar componente `Button` (para cerrar al hacer click fuera)

### 2. Configurar Elementos del Panel

#### Imagen de la Carta:
- Crear GameObject hijo de "ContentPanel"
- Nombrarlo "CardImage"
- Agregar componente `Image`
- Configurar tamaño apropiado (ej: 200x300)

#### Texto del Nombre:
- Crear GameObject hijo de "ContentPanel"
- Nombrarlo "CardNameText"
- Agregar componente `TextMeshPro - Text (UI)`
- Configurar fuente, tamaño y estilo

#### Texto de Descripción:
- Crear GameObject hijo de "ContentPanel"
- Nombrarlo "CardDescriptionText"
- Agregar componente `TextMeshPro - Text (UI)"
- Configurar para texto multilínea

#### Texto de Efecto:
- Crear GameObject hijo de "ContentPanel"
- Nombrarlo "CardEffectText"
- Agregar componente `TextMeshPro - Text (UI)"
- Configurar para texto multilínea con colores

#### Botón de Cerrar:
- Crear GameObject hijo de "ContentPanel"
- Nombrarlo "CloseButton"
- Agregar componente `Button`
- Agregar texto hijo con "X" o "Cerrar"

### 3. Configurar el Script CardInfoPanel

En el componente `CardInfoPanel` del GameObject "CardInfoPanel":

1. **Panel References:**
   - Panel Object: Arrastrar "ContentPanel"
   - Card Image: Arrastrar "CardImage"
   - Card Name Text: Arrastrar "CardNameText"
   - Card Description Text: Arrastrar "CardDescriptionText"
   - Card Effect Text: Arrastrar "CardEffectText"
   - Close Button: Arrastrar "CloseButton"
   - Background Close Button: Arrastrar "ContentPanel" (el panel principal)

2. **Panel Settings:**
   - Animation Duration: 0.3 (duración de la animación)
   - Scale Animation Curve: Configurar curva de animación

### 4. Configurar el Canvas Group

En el GameObject "CardInfoPanel":
- Agregar componente `CanvasGroup`
- Configurar Alpha inicial en 0
- Configurar Blocks Raycasts en true

## Funcionalidad

### Comportamiento:
1. **Click Derecho en Carta:** Abre el panel con información detallada
2. **Bloqueo de Interacciones:** Todas las demás acciones se bloquean
3. **Cerrar Panel:** Click en botón X o fuera del panel
4. **Restauración:** Al cerrar, todas las interacciones vuelven a funcionar

### Información Mostrada:
- **Imagen de la carta**
- **Nombre de la carta**
- **Descripción original**
- **Efecto detallado con valores calculados:**
  - Valor base
  - Multiplicadores aplicados
  - Valor final del efecto

### Tipos de Cartas Soportados:
- **Ataque:** Muestra daño calculado
- **Defensa:** Muestra porcentaje de reducción
- **Curación:** Muestra cantidad de vida restaurada

## Notas Importantes

1. **Singleton Pattern:** El panel usa singleton para acceso global
2. **DontDestroyOnLoad:** El panel persiste entre escenas
3. **Animaciones:** Incluye animaciones suaves de apertura/cierre
4. **Bloqueo Inteligente:** Solo bloquea cuando es necesario
5. **Compatibilidad:** Funciona con el sistema de drag & drop existente

## Solución de Problemas

### Panel no aparece:
- Verificar que CardInfoPanel.Instance no sea null
- Comprobar que el Canvas tenga Sort Order alto
- Verificar que el GameObject "CardInfoPanel" esté activo

### Interacciones no se bloquean:
- Verificar que GameManager.Instance esté disponible
- Comprobar que HandManager.Instance esté disponible
- Revisar logs de consola para errores

### Panel no se cierra:
- Verificar que los botones tengan el componente Button
- Comprobar que los eventos estén asignados correctamente
- Verificar que el CanvasGroup esté configurado
