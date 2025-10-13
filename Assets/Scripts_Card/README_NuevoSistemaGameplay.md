# Nuevo Sistema de Gameplay - Documentación

## Descripción General
Se ha implementado un nuevo sistema de gameplay que cambia completamente la mecánica de combate. El sistema ahora funciona con cartas de asistencia y botones de gatillo.

## Flujo del Nuevo Sistema

### 1. Inicio del Combate
- Se mantienen las funciones de reinicio del deck y robo como antes
- Se inicializan tanto el deck de cartas normales como el deck de cartas de asistencia

### 2. Turno del Jugador

#### Fase 1: Cartas de Asistencia
- **Robar**: El jugador roba automáticamente 4 cartas de asistencia
- **Seleccionar**: El jugador puede seleccionar entre 0 y 4 cartas de asistencia
- **Interacción**: Las cartas se pueden seleccionar haciendo clic o arrastrándolas
- **Efectos**: Las cartas de asistencia tienen efectos acumulables para la acción "gatillo"

#### Fase 2: Botones de Gatillo
- **Confirmar**: Después de seleccionar las cartas de asistencia, aparecen 3 botones:
  - **Ataque**: Ejecuta acción de ataque + efectos acumulables
  - **Bloqueo**: Ejecuta acción de bloqueo + efectos acumulables
  - **Cura**: Ejecuta acción de cura + efectos acumulables

#### Fase 3: Fin de Turno
- Se ejecuta la acción del gatillo seleccionado con los efectos acumulables
- Se descartan todas las cartas de asistencia restantes
- Termina el turno del jugador

### 3. Turno del Enemigo
- Funciona igual que antes
- Al terminar, vuelve al turno del jugador

## Componentes del Sistema

### GameManager
- **Nuevos Estados**: `SelectingAssistance`, `SelectingTrigger`
- **Nuevas Variables**:
  - `allAssistanceCards`: Lista de todas las cartas de asistencia
  - `currentAssistanceHand`: Cartas de asistencia en la mano
  - `selectedAssistanceCards`: Cartas de asistencia seleccionadas
  - `triggerButtonsPanel`: Panel con los botones de gatillo

### AssistanceCard (ScriptableObject)
- **Propiedades**:
  - `cardName`: Nombre de la carta
  - `description`: Descripción del efecto
  - `assistanceType`: Tipo de asistencia (DoubleEffect, etc.)
  - `effectMultiplier`: Multiplicador del efecto
  - `cardColor`: Color de la carta
  - `textColor`: Color del texto

### AssistanceHandManager
- Maneja la UI de las cartas de asistencia
- Controla la selección múltiple
- Maneja el botón de confirmación

### AssistanceCardDisplay
- Muestra cartas de asistencia individuales
- Implementa drag and drop
- Maneja efectos visuales (hover, selección)

## Cartas de Asistencia Disponibles

### Duplicar Efecto
- **Tipo**: DoubleEffect
- **Efecto**: Duplica el efecto del gatillo seleccionado
- **Acumulable**: Sí (múltiples cartas duplican el efecto exponencialmente)
- **Multiplicador**: 2.0x

## Configuración en Unity

### 1. GameManager
1. Asignar `allAssistanceCards` con las cartas de asistencia
2. Asignar `triggerButtonsPanel` con el panel de botones
3. Configurar `attackTriggerButton`, `blockTriggerButton`, `healTriggerButton`

### 2. AssistanceHandManager
1. Crear GameObject con AssistanceHandManager
2. Asignar `assistanceCardPrefab` con el prefab de carta de asistencia
3. Asignar `assistanceHandContainer` con el contenedor de cartas
4. Asignar `confirmButton` con el botón de confirmación

### 3. Botones de Gatillo
1. Crear 3 botones (Ataque, Bloqueo, Cura)
2. Asignar los métodos:
   - `GameManager.OnAttackTriggerClicked()`
   - `GameManager.OnBlockTriggerClicked()`
   - `GameManager.OnHealTriggerClicked()`

### 4. Crear Cartas de Asistencia
1. Usar `AssistanceCardCreator` para crear cartas automáticamente
2. O crear manualmente usando el menú: Create > Card Game > Assistance Card

## Ventajas del Nuevo Sistema

1. **Estrategia**: Permite combinar múltiples efectos
2. **Flexibilidad**: Seleccionar 0-4 cartas según la situación
3. **Acumulación**: Los efectos se pueden combinar
4. **Simplicidad**: Botones claros para las acciones principales
5. **Visual**: Sistema de drag and drop intuitivo

## Notas de Implementación

- Las cartas de asistencia no terminan el turno al jugarlas
- Solo los botones de gatillo terminan el turno
- Los efectos se acumulan multiplicativamente
- Al final de cada turno, todas las cartas de asistencia se descartan
- Al inicio de cada turno, se roban 4 nuevas cartas de asistencia
