# Sistema de Texto de Cambios de Vida - Montaje Manual

## Descripción
Este sistema muestra texto flotante cuando los personajes pierden o recuperan vida, con colores diferenciados (rojo para daño, verde para curación) y animaciones suaves.

## Componentes del Sistema

### 1. HealthChangeTextManager
- **Función**: Gestiona la creación, destrucción y pool de textos de cambios de vida
- **Ubicación**: GameObject separado en la escena
- **Configuración**: Colores, duración, velocidad de flotación, etc.

### 2. HealthChangeText
- **Función**: Script individual para cada texto que se muestra
- **Ubicación**: Componente de cada GameObject de texto
- **Configuración**: Animación, desvanecimiento, movimiento

### 3. HealthChangeTextDisplay
- **Función**: Se conecta a los HealthSystem para detectar cambios
- **Ubicación**: Componente del Player y Enemy
- **Configuración**: Punto de spawn, offset, tipos de texto a mostrar

## Montaje Manual en Escena

### Paso 1: Crear el Prefab del Texto
1. **Crear GameObject vacío** llamado "HealthChangeTextPrefab"
2. **Agregar Canvas**:
   - Render Mode: World Space
   - World Camera: Arrastrar la cámara principal
3. **Agregar CanvasScaler**:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
4. **Agregar CanvasGroup**:
   - Alpha: 1
   - Interactable: false
   - Blocks Raycasts: false
5. **Crear GameObject hijo** llamado "Text"
6. **Agregar TextMeshProUGUI** al hijo "Text":
   - Text: "0"
   - Font Size: Configurar manualmente (ej: 24)
   - Font Style: Configurar manualmente (ej: Bold)
   - Alignment: Configurar manualmente (ej: Center)
   - Auto Size: Configurar manualmente (ej: false)
   - Color: White (se cambiará dinámicamente)
7. **Agregar script** `HealthChangeText` al GameObject padre
8. **Configurar referencias** en el inspector:
   - Text Component: Arrastrar el TextMeshProUGUI
   - Canvas Group: Arrastrar el CanvasGroup
9. **Convertir a Prefab**: Arrastrar a la carpeta Prefabs

### Paso 2: Crear el HealthChangeTextManager
1. **Crear GameObject vacío** llamado "HealthChangeTextManager"
2. **Agregar script** `HealthChangeTextManager`
3. **Configurar en el inspector**:
   - **Health Change Text Prefab**: Arrastrar el prefab creado en el Paso 1
   - **Text Lifetime**: 2 (duración total del texto)
   - **Text Fade Time**: 0.5 (tiempo de desvanecimiento)
   - **Text Float Distance**: 50 (distancia que flota hacia arriba)
   - **Damage Color**: Rojo (Color.red)
   - **Heal Color**: Verde (Color.green)

### Paso 3: Configurar el Player
1. **Seleccionar** el GameObject del Player
2. **Agregar script** `HealthChangeTextDisplay`
3. **Configurar en el inspector**:
   - **Health System**: Arrastrar el HealthSystem del Player
   - **Text Spawn Point**: Dejar vacío (usará la posición del Player)
   - **Text Offset**: Vector3(1, 2, 0) (derecha y arriba)
   - **Show Damage**: ✓ (marcar para mostrar texto de daño)
   - **Show Heal**: ✓ (marcar para mostrar texto de curación)

### Paso 4: Configurar el Enemy
1. **Seleccionar** el GameObject del Enemy
2. **Agregar script** `HealthChangeTextDisplay`
3. **Configurar en el inspector**:
   - **Health System**: Arrastrar el HealthSystem del Enemy
   - **Text Spawn Point**: Dejar vacío (usará la posición del Enemy)
   - **Text Offset**: Vector3(-1, 2, 0) (izquierda y arriba)
   - **Show Damage**: ✓ (marcar para mostrar texto de daño)
   - **Show Heal**: ✓ (marcar para mostrar texto de curación)

### Paso 5: Verificar Configuración
1. **Asegurar** que el HealthChangeTextManager está en la escena
2. **Verificar** que el prefab tiene todos los componentes necesarios
3. **Comprobar** que Player y Enemy tienen HealthChangeTextDisplay
4. **Confirmar** que los HealthSystem están asignados correctamente

## Configuración del Prefab

### Estructura del Prefab
```
HealthChangeTextPrefab (GameObject)
├── Canvas (World Space)
├── CanvasScaler
├── CanvasGroup
├── HealthChangeText (Script)
└── Text (GameObject)
    └── TextMeshProUGUI
```

### Configuración del Canvas
- **Render Mode**: World Space
- **World Camera**: Cámara principal de la escena
- **Plane Distance**: 1
- **Sorting Layer**: UI (o el que prefieras)

### Configuración del TextMeshProUGUI
- **Font Asset**: TextMeshPro Font Asset
- **Font Size**: Configurar manualmente (ej: 24)
- **Font Style**: Configurar manualmente (ej: Bold)
- **Alignment**: Configurar manualmente (ej: Center)
- **Auto Size**: Configurar manualmente (ej: false)
- **Color**: White (se cambiará dinámicamente)

**Nota**: Todas las configuraciones de fuente se hacen manualmente en el prefab. El script no modifica estos valores.

## Personalización

### Cambiar Colores
1. **Seleccionar** HealthChangeTextManager
2. **Modificar** Damage Color y Heal Color en el inspector
3. **Los cambios se aplican inmediatamente**

### Ajustar Animación
1. **Seleccionar** HealthChangeTextManager
2. **Modificar**:
   - Text Lifetime: Duración total del texto
   - Text Fade Time: Tiempo de desvanecimiento
   - Text Float Distance: Distancia de flotación

### Cambiar Posición del Texto
1. **Seleccionar** Player o Enemy
2. **Modificar** Text Offset en HealthChangeTextDisplay
3. **Valores recomendados**:
   - Player: Vector3(1, 2, 0)
   - Enemy: Vector3(-1, 2, 0)

### Personalizar Fuente del Texto
1. **Seleccionar** el prefab HealthChangeTextPrefab
2. **Modificar** el TextMeshProUGUI hijo "Text"
3. **Configurar**:
   - Font Asset
   - Font Size
   - Font Style
   - Alignment
   - Auto Size
   - Y cualquier otra propiedad de texto

## Funcionamiento del Sistema

### Flujo de Datos
1. **HealthSystem** detecta cambio de vida (TakeDamage/Heal)
2. **HealthChangeTextDisplay** recibe el evento
3. **HealthChangeTextManager** crea texto desde el pool
4. **HealthChangeText** anima y muestra el texto
5. Texto se devuelve al pool al finalizar

### Eventos Utilizados
- `OnTakeDamage`: Se dispara cuando se recibe daño
- `OnHeal`: Se dispara cuando se recupera vida

### Pool de Objetos
- **Tamaño**: 10 objetos por defecto
- **Ventaja**: Mejora el rendimiento evitando crear/destruir objetos
- **Gestión**: Automática, no requiere configuración manual

## Solución de Problemas

### El texto no aparece
1. **Verificar** que HealthChangeTextManager existe en la escena
2. **Verificar** que HealthChangeTextPrefab está asignado en el manager
3. **Verificar** que HealthChangeTextDisplay está en Player/Enemy
4. **Verificar** que HealthSystem tiene eventos OnTakeDamage/OnHeal
5. **Verificar** que la cámara está configurada en el Canvas del prefab

### El texto aparece en posición incorrecta
1. **Ajustar** "Text Offset" en HealthChangeTextDisplay
2. **Verificar** que "Text Spawn Point" está configurado correctamente
3. **Asegurar** que el Canvas está en modo World Space

### El texto no se desvanece
1. **Verificar** que CanvasGroup está presente en el prefab
2. **Verificar** que las corrutinas no están siendo interrumpidas
3. **Verificar** que el GameObject no se destruye prematuramente

### Error de prefab no asignado
1. **Verificar** que el prefab está creado correctamente
2. **Asegurar** que el prefab tiene todos los componentes necesarios
3. **Verificar** que la referencia está asignada en HealthChangeTextManager

### Problemas con la fuente del texto
1. **Verificar** que el TextMeshProUGUI está configurado correctamente
2. **Asegurar** que el Font Asset está asignado
3. **Configurar manualmente** todas las propiedades de fuente en el prefab

## Notas Importantes

- **El sistema funciona automáticamente** una vez configurado correctamente
- **No interfiere** con el sistema de vida existente
- **Utiliza pool de objetos** para mejor rendimiento
- **Los textos se muestran en coordenadas de mundo** (World Space)
- **Se puede personalizar completamente** desde el inspector
- **Compatible** con el sistema de cartas existente
- **Requiere TextMeshPro** para funcionar correctamente
- **Las configuraciones de fuente se hacen manualmente** en el prefab

## Verificación Final

Antes de probar, asegúrate de que:
- ✅ HealthChangeTextManager está en la escena
- ✅ HealthChangeTextPrefab está asignado en el manager
- ✅ Player tiene HealthChangeTextDisplay configurado
- ✅ Enemy tiene HealthChangeTextDisplay configurado
- ✅ Los HealthSystem están asignados correctamente
- ✅ El prefab tiene todos los componentes necesarios
- ✅ La cámara está configurada en el Canvas del prefab
- ✅ El TextMeshProUGUI está configurado manualmente
