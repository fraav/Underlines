# Nuevas Funcionalidades de Animación - Sistema de Cartas

## Resumen de Cambios

Se han implementado nuevas funcionalidades de animación para mejorar la experiencia del jugador y la sincronización entre acciones del jugador y enemigo.

## Nuevas Animaciones del Jugador

### 1. Animación de Bloqueo Activo (BlockIdle)
- **Trigger**: `BlockIdle`
- **Propósito**: Mantiene al jugador en estado de bloqueo visual
- **Activación**: Se ejecuta cuando se activa el bloqueo pendiente
- **Desactivación**: Se detiene cuando se desactiva el bloqueo

### 2. Animación de Daño Mientras Bloquea
- **Trigger**: `TakeDamageWhileBlocking`
- **Propósito**: Muestra al jugador recibiendo daño mientras mantiene la postura de bloqueo
- **Activación**: Se ejecuta cuando el jugador recibe daño y tiene bloqueo activo

### 3. Animación de Daño Normal
- **Trigger**: `TakeDamage`
- **Propósito**: Muestra al jugador recibiendo daño cuando no está bloqueando
- **Activación**: Se ejecuta cuando el jugador recibe daño sin bloqueo activo

## Sistema de Bloqueo Mejorado

### Comportamiento del Bloqueo
1. **Activación**: Al usar carta de bloqueo, se marca como "pendiente"
2. **Activación Real**: Se activa cuando el enemigo inicia su ataque
3. **Persistencia**: Se mantiene activo durante un tiempo configurable (por defecto 3 segundos)
4. **Desactivación Automática**: Se desactiva automáticamente si no recibe daño
5. **Desactivación por Daño**: Se mantiene activo incluso después de recibir daño

### Configuración del Bloqueo
- **Tiempo de Auto-Desactivación**: Configurable en `blockAutoDeactivateTime` (3.0f por defecto)
- **Animación de Bloqueo**: `blockAnimationTrigger` para la animación inicial
- **Animación de Bloqueo Activo**: `blockIdleAnimationTrigger` para mantener el estado

## Mejoras en Animaciones del Enemigo

### Animación de Daño
- **Duración Configurable**: `damageAnimationDuration` (0.5f por defecto)
- **Sincronización de Sonido**: Mejorada para reproducir sonidos en momentos exactos
- **Manejo de Tiempo**: La animación espera a completarse antes de continuar

## Configuración en Unity

### PlayerController
```csharp
[Header("Block System")]
[SerializeField] private string blockAnimationTrigger = "Block";
[SerializeField] private string blockIdleAnimationTrigger = "BlockIdle";
[SerializeField] private float blockAutoDeactivateTime = 3.0f;

[Header("Damage Animations")]
[SerializeField] private string damageAnimationTrigger = "TakeDamage";
[SerializeField] private string damageWhileBlockingAnimationTrigger = "TakeDamageWhileBlocking";
```

### EnemyController
```csharp
[Header("Damage Animations")]
[SerializeField] private string damageAnimationTrigger = "TakeDamage";
[SerializeField] private float damageAnimationDuration = 0.5f;
```

## Flujo de Funcionamiento

### 1. Jugador Usa Carta de Bloqueo
- Se marca bloqueo como "pendiente"
- Se reproduce animación de bloqueo inicial
- Se inicia temporizador de auto-desactivación

### 2. Enemigo Ataca
- Se activa bloqueo pendiente del jugador
- Se reproduce animación de bloqueo activo
- Se aplica reducción de daño
- Se reproduce sonido de bloqueo

### 3. Jugador Recibe Daño
- Si está bloqueando: animación de daño mientras bloquea
- Si no está bloqueando: animación de daño normal
- El bloqueo se mantiene activo

### 4. Fin del Turno del Enemigo
- Se notifica al GameManager
- El bloqueo se desactiva automáticamente después del tiempo configurado

## Ventajas del Nuevo Sistema

1. **Mejor Feedback Visual**: El jugador siempre sabe cuándo está bloqueando
2. **Sincronización Mejorada**: Las animaciones y sonidos están perfectamente sincronizados
3. **Persistencia del Bloqueo**: El bloqueo se mantiene activo hasta que sea necesario
4. **Desactivación Inteligente**: Se desactiva automáticamente cuando no es necesario
5. **Animaciones Contextuales**: Diferentes animaciones según el estado del jugador

## Notas de Implementación

- Todas las funcionalidades existentes se mantienen intactas
- El sistema es retrocompatible con configuraciones anteriores
- Los tiempos de animación son configurables desde el inspector
- El sistema maneja automáticamente la limpieza de recursos y corrutinas
