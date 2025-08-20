# 🎯 Guía de Configuración de Sprites de Selección de Objetivo

## Resumen de la Funcionalidad

Se ha implementado un sistema visual que enciende automáticamente un sprite hijo del objetivo válido cuando se presiona una carta, indicando claramente qué objetivo puede ser seleccionado.

## 🎮 Cómo Funciona

### **Flujo de Selección:**
1. **Jugador presiona una carta** → Se determina el objetivo válido
2. **Se enciende el sprite** del objetivo válido (Player o Enemy)
3. **Jugador selecciona objetivo** → Se ejecuta la acción
4. **Sprite se apaga** automáticamente

### **Tipos de Carta y Objetivos:**
- **🗡️ Ataque**: Solo enemigo válido → `enemyTargetSprite` se enciende
- **🛡️ Bloqueo**: Solo jugador válido → `playerTargetSprite` se enciende  
- **💚 Curación**: Solo jugador válido → `playerTargetSprite` se enciende

## 🔧 Configuración en Unity

### **1. Preparar los Sprites:**
- Crear sprites visuales para indicar selección (ej: flechas, círculos, etc.)
- Hacerlos hijos del GameObject correspondiente:
  - `playerTargetSprite` → Hijo del Player
  - `enemyTargetSprite` → Hijo del Enemy

### **2. Configurar en GameManager:**
```
[Header("Target Selection Sprites")]
Player Target Sprite: [Arrastrar el sprite del Player]
Enemy Target Sprite: [Arrastrar el sprite del Enemy]
```

### **3. Configuración de los Sprites:**
- **Posición**: Centrados en el objetivo
- **Escala**: Ajustar según el tamaño del personaje
- **Orden de Capa**: Asegurar que estén por encima del personaje
- **Estado Inicial**: Desactivados por defecto

## 📱 Ejemplo de Configuración

### **Player Target Sprite:**
```
Player (GameObject)
├── Sprite Renderer (Personaje)
├── Player Target Sprite ← Añadir aquí
│   ├── Sprite Renderer
│   └── Imagen del sprite de selección
└── Otros componentes...
```

### **Enemy Target Sprite:**
```
Enemy (GameObject)
├── Sprite Renderer (Enemigo)
├── Enemy Target Sprite ← Añadir aquí
│   ├── Sprite Renderer
│   └── Imagen del sprite de selección
└── Otros componentes...
```

## 🎨 Ideas para Sprites de Selección

### **Estilos Visuales:**
- **Flechas apuntando** al objetivo
- **Círculos pulsantes** alrededor del objetivo
- **Auras brillantes** o efectos de partículas
- **Iconos específicos** (espada para ataque, escudo para bloqueo)

### **Animaciones Recomendadas:**
- **Pulsación suave** (Scale up/down)
- **Rotación lenta** para llamar la atención
- **Cambio de color** o transparencia
- **Efectos de partículas** sutiles

## ⚙️ Configuración Avanzada

### **Personalización por Tipo de Carta:**
Si quieres sprites diferentes según el tipo de carta, puedes modificar el código:

```csharp
// En StartTargetSelection, añadir lógica específica:
switch (card.cardType)
{
    case CardData.CardType.Attack:
        // Sprite de ataque para enemigo
        break;
    case CardData.CardType.Block:
        // Sprite de bloqueo para jugador
        break;
    case CardData.CardType.Heal:
        // Sprite de curación para jugador
        break;
}
```

### **Múltiples Sprites:**
Puedes tener varios sprites por objetivo:
```csharp
[Header("Target Selection Sprites")]
[SerializeField] private GameObject[] playerTargetSprites;
[SerializeField] private GameObject[] enemyTargetSprites;
```

## 🔍 Solución de Problemas

### **Sprite no se enciende:**
- Verificar que esté asignado en el inspector del GameManager
- Confirmar que el GameObject esté activo en la jerarquía
- Verificar que no haya errores en la consola

### **Sprite no se apaga:**
- Verificar que se llame `CancelSelection()` o `SelectTarget()`
- Confirmar que el estado del turno cambie correctamente

### **Sprite aparece en el lugar incorrecto:**
- Ajustar la posición del sprite hijo
- Verificar que sea hijo del GameObject correcto
- Ajustar el orden de capa (Sorting Layer)

## ✅ Ventajas del Sistema

1. **Feedback Visual Claro**: El jugador siempre sabe qué objetivo seleccionar
2. **Automático**: Se activa/desactiva sin intervención manual
3. **Configurable**: Fácil de personalizar desde el inspector
4. **Integrado**: Funciona perfectamente con el sistema existente
5. **Eficiente**: Solo se activa cuando es necesario

## 🎯 Resultado Esperado

- **Carta de Ataque** → Sprite del enemigo se enciende
- **Carta de Bloqueo** → Sprite del jugador se enciende
- **Carta de Curación** → Sprite del jugador se enciende
- **Selección o Cancelación** → Todos los sprites se apagan
- **Transiciones suaves** entre estados de selección
