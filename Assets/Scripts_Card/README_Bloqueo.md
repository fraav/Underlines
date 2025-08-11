# Sistema de Bloqueo - Documentación

## Descripción General
Este sistema implementa un mecanismo de bloqueo para el jugador que reduce el daño recibido de los ataques enemigos. El sistema incluye animaciones, efectos de sonido y sincronización entre el jugador y el enemigo.

## Componentes del Sistema

### 1. PlayerController
- **Campos de Bloqueo:**
  - `blockAnimationTrigger`: Trigger del Animator para la animación de bloqueo
  - `blockHitSound`: Clip de audio que se reproduce cuando se bloquea un ataque
- **Campos de Daño:**
  - `damageAnimationTrigger`: Trigger del Animator para la animación de recibir daño
  - `damageSound`: Clip de audio que se reproduce cuando se recibe daño

### 2. EnemyController
- **Campos de Daño:**
  - `damageAnimationTrigger`: Trigger del Animator para la animación de recibir daño
  - `damageSound`: Clip de audio que se reproduce cuando se recibe daño

### 3. GameManager
- **Métodos de Sincronización:**
  - `OnEnemyAttackStart()`: Se llama cuando el enemigo inicia su animación de ataque
  - `OnEnemyAttackApplied()`: Se llama cuando el enemigo aplica su ataque (en el punto de acción)
- **Métodos de Animación de Daño:**
  - `PlayPlayerDamageAnimation()`: Reproduce la animación de daño del jugador
  - `PlayEnemyDamageSound()`: Reproduce el sonido de daño del enemigo

## Configuración en Unity

### PlayerController
1. Asignar el Animator del jugador
2. Configurar el trigger de animación de bloqueo
3. Asignar el clip de audio de bloqueo
4. Configurar el trigger de animación de daño
5. Asignar el clip de audio de daño

### EnemyController
1. Asignar el Animator del enemigo
2. Configurar el trigger de animación de daño
3. Asignar el clip de audio de daño

### GameManager
1. Asignar el AudioSource principal
2. Configurar todos los clips de audio necesarios

## Flujo Operacional

### 1. Activación del Bloqueo
1. El jugador juega una carta de bloqueo
2. Se activa el estado de bloqueo en `PlayerController`
3. Se reproduce la animación de bloqueo
4. Se reproduce el sonido de la carta

### 2. Ataque Enemigo con Bloqueo
1. El enemigo inicia su turno
2. Se llama `GameManager.OnEnemyAttackStart()`
3. Si el jugador tiene bloqueo activo, se reproduce la animación de bloqueo
4. La animación de ataque del enemigo y la de bloqueo del jugador se ejecutan simultáneamente
5. Se aplica el daño reducido
6. Se reproduce el sonido de bloqueo del jugador

### 3. Ataque del Jugador
1. El jugador juega una carta de ataque
2. Se reproduce la animación de ataque
3. Se aplica el daño al enemigo
4. Se reproduce el sonido de la carta

### 4. Curación del Jugador
1. El jugador juega una carta de curación
2. Se reproduce la animación de curación
3. Se aplica la curación
4. Se reproduce el sonido de la carta

## Ventajas de la Sincronización

### Animaciones Simultáneas
- **Bloqueo del Jugador + Ataque del Enemigo**: Ambas animaciones se ejecutan al mismo tiempo, creando una experiencia visual más fluida y realista.
- **Mejor Feedback Visual**: El jugador puede ver inmediatamente que su bloqueo está funcionando.

## Sistema de Animaciones de Daño

### Jugador
- **Trigger de Animación**: `damageAnimationTrigger` (por defecto: "TakeDamage")
- **Sonido**: `damageSound` - se reproduce automáticamente cuando se recibe daño
- **Método**: `PlayDamageAnimation()` - puede ser llamado externamente para reproducir la animación

### Enemigo
- **Trigger de Animación**: `damageAnimationTrigger` (por defecto: "TakeDamage")
- **Sonido**: `damageSound` - se reproduce automáticamente cuando se recibe daño
- **Método**: `PlayDamageAnimation()` - puede ser llamado externamente para reproducir la animación

## Debugging

### Logs de Sistema
- Activación/desactivación de bloqueo
- Aplicación de reducción de daño
- Ejecución de animaciones
- Reproducción de sonidos

### Verificación de Estado
- `playerController.HasBlockActive()`: Verifica si el bloqueo está activo
- `playerController.GetBlockReductionMultiplier()`: Obtiene el multiplicador de reducción actual

## Consideraciones Técnicas

### Eventos Unity
- `healthSystem.OnTakeDamage`: Se suscribe para detectar cuando el jugador recibe daño
- `OnEnemyAttack` y `OnEnemyHeal`: Eventos del enemigo para acciones específicas

### Corrutinas
- `PerformCardAnimation`: Maneja el timing de animaciones y sonidos de cartas
- `PerformAction`: Maneja el timing de acciones enemigas

### Singleton Pattern
- `GameManager.Instance`: Acceso global al sistema de gestión del juego

## Mejoras Futuras

### Animaciones de Daño
- Sistema de animaciones de daño para jugador y enemigo
- Referencias de código listas para futuras implementaciones
- Campos de inspector disponibles para asignar animaciones

### Sistema de Partículas
- Los efectos de partículas fueron removidos por simplicidad
- El sistema está optimizado para rendimiento y claridad 