# 🎭 Guía de Configuración del Animator Controller

## Configuración para BlockIdle con Transición Automática

### 📋 **Estructura del Animator Controller**

```
┌─────────────────────────────────────────────────────────────┐
│                    ANIMATOR CONTROLLER                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────┐    ┌─────────────┐    ┌─────────────┐        │
│  │  IDLE   │◄──►│ BLOCK IDLE  │◄──►│    BLOCK    │        │
│  │(Default)│    │ (Bool)      │    │ (Trigger)   │        │
│  └─────────┘    └─────────────┘    └─────────────┘        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 🔧 **Configuración de Estados**

#### **1. Estado IDLE (Por Defecto)**
- **Animation**: Tu animación de idle por defecto
- **Speed**: 1
- **Loop Time**: ✅ True
- **Entry State**: ✅ True (marcar como estado de entrada)

#### **2. Estado BLOCK IDLE**
- **Animation**: Tu animación de bloqueo activo
- **Speed**: 1
- **Loop Time**: ✅ True

#### **3. Estado BLOCK**
- **Animation**: Tu animación de bloqueo inicial
- **Speed**: 1
- **Loop Time**: ❌ False

### 🔄 **Configuración de Transiciones**

#### **Transición: IDLE → BLOCK IDLE**
- **Source**: IDLE
- **Destination**: BLOCK IDLE
- **Conditions**: 
  - `BlockIdle` (Bool) = true
- **Has Exit Time**: ❌ **FALSE** (muy importante!)
- **Transition Duration**: 0.1
- **Transition Offset**: 0

#### **Transición: BLOCK IDLE → IDLE**
- **Source**: BLOCK IDLE
- **Destination**: IDLE
- **Conditions**: 
  - `BlockIdle` (Bool) = false
- **Has Exit Time**: ❌ **FALSE** (muy importante!)
- **Transition Duration**: 0.1
- **Transition Offset**: 0

#### **Transición: BLOCK IDLE → BLOCK**
- **Source**: BLOCK IDLE
- **Destination**: BLOCK
- **Conditions**: 
  - `Block` (Trigger) = true
- **Has Exit Time**: ❌ **FALSE**
- **Transition Duration**: 0.1
- **Transition Offset**: 0

#### **Transición: BLOCK → BLOCK IDLE**
- **Source**: BLOCK
- **Destination**: BLOCK IDLE
- **Conditions**: 
  - (Sin condiciones - transición automática)
- **Has Exit Time**: ✅ **TRUE**
- **Exit Time**: 0.9 (cuando la animación esté casi terminada)
- **Transition Duration**: 0.1
- **Transition Offset**: 0

### 🎯 **Parámetros del Animator**

#### **Bool Parameters:**
- **`BlockIdle`**: Controla si el jugador está en estado de bloqueo activo

#### **Trigger Parameters:**
- **`Block`**: Activa la animación de bloqueo inicial
- **`TakeDamage`**: Activa la animación de recibir daño
- **`TakeDamageWhileBlocking`**: Activa la animación de daño mientras bloquea

### 📱 **Configuración en Unity Inspector**

#### **PlayerController:**
```
[Header("Block System")]
Block Animation Trigger: "Block"
Block Idle Animation Trigger: "BlockIdle"  ← Usar este nombre exacto
Block Auto Deactivate Time: 3.0
```

### 🔄 **Flujo de Funcionamiento**

1. **Jugador usa carta de bloqueo**
   - Se ejecuta animación `Block`
   - Al terminar, transición automática a `BlockIdle`

2. **Enemigo ataca**
   - Se mantiene en `BlockIdle`
   - Si recibe daño: `TakeDamageWhileBlocking`

3. **Jugador inicia su turno**
   - `BlockIdle` se desactiva (Bool = false)
   - Transición automática de vuelta a `IDLE`

### ⚠️ **Puntos Importantes**

1. **Has Exit Time = FALSE** en las transiciones principales
2. **BlockIdle debe ser un Bool, no un Trigger**
3. **La transición de BLOCK → BLOCK IDLE debe tener Has Exit Time = TRUE**
4. **El estado IDLE debe ser el Entry State**

### 🎮 **Resultado Esperado**

- El jugador bloquea → animación de bloqueo → mantiene postura de bloqueo
- Si el enemigo no ataca → al iniciar turno del jugador, vuelve automáticamente a idle
- Si el enemigo ataca → mantiene la postura de bloqueo durante el ataque
- Transiciones suaves entre todos los estados

### 🔧 **Solución de Problemas**

#### **Si no vuelve a idle:**
- Verificar que `Has Exit Time = FALSE` en la transición BLOCK IDLE → IDLE
- Confirmar que el parámetro `BlockIdle` es Bool, no Trigger
- Verificar que el estado IDLE está marcado como Entry State

#### **Si las transiciones son bruscas:**
- Aumentar `Transition Duration` a 0.2 o 0.3
- Verificar que las animaciones tienen frames de transición suaves

