# Instrucciones de Montaje en Escena - Nuevo Sistema de Combate

Este documento explica cómo montar todos los nuevos elementos necesarios para el correcto funcionamiento del nuevo sistema de combate por turnos.

## 📋 Componentes Necesarios

### 1. **GameManager** (Ya existe, solo verificar)
   - ✅ Debe estar en la escena de batalla
   - ✅ Debe tener referencias a PlayerController y EnemyController
   - ✅ Debe tener configurada la lista `allCards` con todas las cartas del juego

### 2. **PlayerTurnEffects** (NUEVO)
   - **Ubicación**: Crear un GameObject vacío llamado `PlayerTurnEffects` en la jerarquía
   - **Componente**: Agregar el script `PlayerTurnEffects.cs`
   - **Nota**: Este script gestiona los efectos acumulados durante el turno del jugador

### 3. **ActionButtonsController** (NUEVO)
   - **Ubicación**: Crear un GameObject vacío llamado `ActionButtonsController` en la jerarquía
   - **Componente**: Agregar el script `ActionButtonsController.cs`
   - **Configuración**:
     - Crear 3 botones de UI (UI > Button):
       - `AttackButton` - Botón de Ataque
       - `BlockButton` - Botón de Bloqueo
       - `HealButton` - Botón de Curación
     - Asignar los botones en los campos correspondientes del script
     - **Container**: Crear un GameObject padre para los botones llamado `ButtonsContainer` y asignarlo al campo `buttonsContainer`
     - **Valores base**: Configurar los valores por defecto:
       - `baseAttackValue`: 10 (daño base)
       - `baseBlockValue`: 20 (porcentaje de reducción)
       - `baseHealValue`: 15 (curación base)

### 4. **EffectsDisplayUI** (NUEVO)
   - **Ubicación**: Crear un GameObject vacío llamado `EffectsDisplayUI` en la jerarquía
   - **Componente**: Agregar el script `EffectsDisplayUI.cs`
   - **Configuración de UI**:
     - Crear un Panel de UI (UI > Panel) llamado `EffectsPanel`
     - Dentro del panel:
       - Crear un TextMeshPro - Text (UI > Text - TextMeshPro) llamado `EffectsText`
       - Crear un Button (UI > Button) llamado `UndoButton` con texto "Deshacer"
     - Asignar referencias en el script:
       - `effectsPanel` → GameObject EffectsPanel
       - `effectsText` → Componente TMP_Text de EffectsText
       - `undoButton` → Componente Button de UndoButton

### 5. **HandManager** (dos paneles: carta + carta/objeto)
   - **Cartas**: `handContainer` + posición manual horizontal (`cardSpacing`, `startX/Y`)
   - **Carta/objeto**: `objectHandContainer` + posición manual **vertical** (`objectCardSpacing`, `objectStartX/Y`)
   - Mismo sistema que las cartas; **desactiva** Vertical Layout Group en `HandPanel-objeto` (HandManager lo desactiva al iniciar)
   - Ajusta `objectCardSpacing` (ej. 130) y `objectStartYPosition` para apilar hacia abajo sin solaparse

### 5b. **GameManager – dos mazos**
   - `allCards` → solo cartas normales (ataque/bloqueo/cura/booster)
   - `allObjectCards` → solo ScriptableObjects **Object Card** (nunca mezclar listas)
   - Al inicio de cada turno del jugador: **2 cartas + 2 carta/objeto** (`CardsDrawPerTurn` / `ObjectCardsDrawPerTurn` en código)

### 6. **Player GameObject** (Ya existe)
   - ✅ Debe tener el tag "Player"
   - ✅ Debe tener el componente PlayerController
   - ✅ Debe tener el componente HealthSystem
   - ✅ Debe tener un Collider (para detección de raycast al arrastrar cartas)

### 7. **Enemy GameObject** (Ya existe)
   - ✅ Debe tener el tag "Enemy"
   - ✅ Debe tener el componente EnemyController
   - ✅ Debe tener el componente HealthSystem
   - ✅ Debe tener un Collider (para detección de raycast)

## 🎨 Diseño de UI Recomendado

### Panel de Botones de Acción
```
Canvas
└── ActionButtonsContainer
    ├── AttackButton (Texto: "ATAQUE")
    ├── BlockButton (Texto: "BLOQUEO")
    └── HealButton (Texto: "CURACIÓN")
```

**Posición recomendada**: Centro inferior de la pantalla, horizontalmente distribuidos.

### Panel de Efectos Acumulados
```
Canvas
└── EffectsPanel
    ├── EffectsText (TMP_Text, alineación izquierda)
    └── UndoButton (Texto: "Deshacer")
```

**Posición recomendada**: Esquina superior izquierda o derecha.

## 🃏 Carta/objeto (Object Card) – montaje rápido

### Crear assets
1. Project → **Create → Card Game → Object Card**
2. Por cada efecto, configura `effectType`:
   - **DoubleNextAction** → duplica la siguiente acción (botones de ataque/bloqueo/cura)
   - **Heal** → `baseValue` = cantidad de curación
   - **RestoreEnergy** → `restoreEnergyAmount` (por defecto 2)
   - **DrawCard** → `drawCardCount` (por defecto 1, roba del mazo de **cartas**, no del mazo objeto)
3. `playEnergyCost` como en las cartas normales
4. Añade cada asset a **GameManager → All Object Cards** (no a `allCards`)

### Prefab de mano objeto
- Duplica `Card_Prefab` (o crea uno nuevo)
- Quita `Card` / `CardDisplay` si no los necesitas en ese prefab
- Añade **ObjectCardDisplay** y **ObjectCard**
- Enlaza en HandManager → `objectCardPrefab` y `objectHandContainer`

### Jugar carta/objeto
- **Arrastrar al jugador** (tag `Player`) → efecto al instante (igual que cartas booster)
- **No** se activa con clic izquierdo
- Posición en mano: **manual** vía HandManager (como las cartas), no Layout Group activo
- Prefab: `ObjectCard` + `ObjectCardDisplay` (misma base de arrastre que `Card` + `CardDisplay`)

### Tienda (opcional)
- GameObject con `ObjectCardPurchaseShopItem` (hereda de `ShopItem`)
- Asigna el `ObjectCardData` a comprar → llama a `AddCardToPermanentObjectDeck`

### Ejemplo de 4 carta/objeto iniciales
| Nombre (ejemplo) | effectType      | Campos clave        |
|----------------|-----------------|---------------------|
| Duplicador     | DoubleNextAction| playEnergyCost: 1   |
| Botiquín       | Heal            | baseValue: 20       |
| Batería        | RestoreEnergy   | restoreEnergyAmount: 2 |
| Recarga        | DrawCard        | drawCardCount: 1    |

## 📝 Pasos de Montaje Detallados

### Paso 1: Configurar PlayerTurnEffects
1. Crea un GameObject vacío llamado `PlayerTurnEffects`
2. Agrega el componente `PlayerTurnEffects`
3. No requiere configuración adicional (es un singleton)

### Paso 2: Configurar ActionButtonsController
1. Crea un GameObject vacío llamado `ActionButtonsController`
2. Agrega el componente `ActionButtonsController`
3. Crea 3 botones de UI:
   ```
   - AttackButton
   - BlockButton  
   - HealButton
   ```
4. Crea un GameObject padre `ButtonsContainer` y coloca los botones dentro
5. En el script `ActionButtonsController`:
   - Arrastra `AttackButton` al campo `attackButton`
   - Arrastra `BlockButton` al campo `blockButton`
   - Arrastra `HealButton` al campo `healButton`
   - Arrastra `ButtonsContainer` al campo `buttonsContainer`
   - Configura los valores base según tu juego

### Paso 3: Configurar EffectsDisplayUI
1. Crea un GameObject vacío llamado `EffectsDisplayUI`
2. Agrega el componente `EffectsDisplayUI`
3. Crea un Panel de UI llamado `EffectsPanel`
4. Dentro del panel:
   - Crea un TextMeshPro llamado `EffectsText`
   - Crea un Button llamado `UndoButton` con texto "Deshacer"
5. En el script `EffectsDisplayUI`:
   - Arrastra `EffectsPanel` al campo `effectsPanel`
   - Arrastra el componente `TMP_Text` de `EffectsText` al campo `effectsText`
   - Arrastra el componente `Button` de `UndoButton` al campo `undoButton`

### Paso 4: Crear Cartas Potenciadoras
1. En el Project, clic derecho > Create > Card Game > Card
2. Configura la nueva carta:
   - `cardType`: Booster
   - `boosterEffectType`: DoubleAction (o el que desees)
   - `boosterValue`: 1.0 (para DoubleAction, este valor indica cuántas veces se duplica)
   - Asigna nombre, descripción e icono
3. Agrega esta carta a la lista `allCards` del GameManager

**Ejemplo de carta "Duplicar Acción"**:
- `cardName`: "Duplicar Acción"
- `cardType`: Booster
- `boosterEffectType`: DoubleAction
- `boosterValue`: 1.0
- `description`: "La próxima acción se ejecutará dos veces"

## ✅ Verificaciones Finales

1. **GameManager**:
   - ✅ Tiene lista `allCards` configurada
   - ✅ Tiene referencias a PlayerController y EnemyController

2. **PlayerTurnEffects**:
   - ✅ Existe en la escena
   - ✅ Tiene el componente `PlayerTurnEffects`

3. **ActionButtonsController**:
   - ✅ Existe en la escena
   - ✅ Tiene los 3 botones asignados
   - ✅ Tiene el container asignado

4. **EffectsDisplayUI**:
   - ✅ Existe en la escena
   - ✅ Tiene panel, texto y botón asignados

5. **Cartas**:
   - ✅ Se han creado al menos una carta tipo "Booster"
   - ✅ Las cartas están agregadas a `allCards` del GameManager

## 🐛 Debug y Pruebas

El sistema incluye extensos `Debug.Log` para verificar el funcionamiento. Al probar:

1. **Verifica la consola** para ver los mensajes de debug:
   - `[GameManager]` - Operaciones del GameManager
   - `[ActionButtonsController]` - Ejecución de acciones
   - `[PlayerTurnEffects]` - Gestión de efectos
   - `[EffectsDisplayUI]` - Actualización de UI

2. **Flujo de prueba**:
   - Inicia la batalla → Mano con 2 cartas + 2 carta/objeto en sus paneles
   - Arrastra una carta Booster al jugador → Deberías ver "Efecto agregado"
   - Presiona un botón de acción → Deberías ver "Botón de [ACCIÓN] presionado"
   - Verifica que los efectos se muestren en el panel
   - Prueba el botón "Deshacer"

## 📌 Notas Importantes

- **Todas las cartas Booster** deben tener como objetivo válido al **jugador**
- El sistema **descartará todas las cartas** al final del turno
- Se robarán **2 cartas** y **2 carta/objeto** al inicio de cada turno del jugador
- Los **efectos se consumen** cuando se ejecuta una acción
- El **turno del enemigo** no ha sido modificado y funciona igual que antes

## 🔧 Troubleshooting

**Problema**: Los botones no aparecen durante el turno del jugador
- **Solución**: Verifica que `ActionButtonsController.Instance` no sea null y que `UpdateButtonsVisibility()` se esté llamando

**Problema**: Los efectos no se muestran en el panel
- **Solución**: Verifica que `EffectsDisplayUI.Instance` exista y que las referencias de UI estén asignadas

**Problema**: Las cartas no se descartan al final del turno
- **Solución**: Verifica que `DiscardAllHandCards()` se esté llamando en `EndPlayerTurn()`

**Problema**: No se roban cartas al inicio del turno
- **Solución**: Verifica que `DrawCardsForNewTurn()` se esté llamando en `StartPlayerTurn()`

