# Sistema Final de Cartas de Asistencia

## 🎯 **Descripción del Sistema**
El sistema mantiene el drag & drop original que funcionaba correctamente, pero ahora todas las cartas son de asistencia. Se arrastran en grupo al objetivo válido, y luego se usan botones en pantalla para ejecutar las animaciones y efectos.

## 🎮 **Flujo del Sistema**

### **1. Inicio del Turno**
- Se roban 4 cartas de asistencia
- Aparece el botón "Confirmar Selección"
- Las cartas se pueden arrastrar al objetivo válido

### **2. Selección de Cartas**
- **Arrastrar cartas**: Se arrastran al objetivo válido (Player/Enemy)
- **Selección múltiple**: Se pueden seleccionar 0-4 cartas
- **Feedback visual**: Las cartas seleccionadas se resaltan
- **No termina turno**: Las cartas de asistencia no terminan el turno inmediatamente

### **3. Confirmación**
- **Botón "Confirmar Selección"**: Aparece cuando hay cartas de asistencia
- **Hacer clic**: Confirma la selección y muestra botones de gatillo

### **4. Botones de Gatillo**
- **3 botones**: ATAQUE, BLOQUEO, CURA
- **Elegir uno**: Según la estrategia deseada
- **Efectos acumulables**: Se aplican los efectos de las cartas de asistencia seleccionadas

### **5. Fin de Turno**
- Se ejecuta la animación y efecto correspondiente
- Se descartan todas las cartas restantes
- Termina el turno del jugador

---

## 🔧 **Cambios Realizados**

### **✅ Mantenido del Sistema Original:**
- **Card.cs**: Drag & drop original restaurado
- **HandManager.cs**: Sistema de cartas original restaurado
- **Posicionamiento**: Las cartas mantienen su posición original en la mano
- **Interacciones**: Mismo comportamiento de drag & drop

### **🆕 Cambios Mínimos:**
- **GameManager.StartTargetSelection()**: Detecta cartas de asistencia
- **GameManager.SelectTarget()**: No ejecuta acción inmediata para asistencia
- **HandManager**: Solo agregado botón de confirmación
- **CardData**: Campos para cartas de asistencia

---

## 📋 **Configuración**

### **1. Crear Cartas de Asistencia**
```
1. Create → Card Game → Card
2. Configurar:
   - Card Type: Assistance
   - Assistance Type: DoubleEffect (o el que quieras)
   - Effect Multiplier: 2.0
   - Card Color: Magenta
   - Text Color: White
```

### **2. Configurar GameManager**
```
- All Cards: Agregar las cartas de asistencia
- Trigger Buttons Panel: Panel con 3 botones
- Attack Trigger Button: Botón ATAQUE
- Block Trigger Button: Botón BLOQUEO  
- Heal Trigger Button: Botón CURA
```

### **3. Configurar HandManager**
```
- Confirm Button: Botón "Confirmar Selección"
```

---

## 🎯 **Tipos de Cartas de Asistencia**

### **DoubleEffect**
- **Efecto**: Duplica el efecto del gatillo
- **Multiplicador**: 2.0
- **Acumulable**: Sí (múltiples cartas = efecto exponencial)

### **ExtraDamage**
- **Efecto**: Aumenta daño del gatillo de ataque
- **Multiplicador**: 1.5

### **ExtraBlock**
- **Efecto**: Mejora bloqueo del gatillo de bloqueo
- **Multiplicador**: 1.3

### **ExtraHeal**
- **Efecto**: Aumenta curación del gatillo de cura
- **Multiplicador**: 1.5

---

## 🚀 **Ventajas del Sistema**

### **✅ Mantiene lo que funcionaba:**
- Drag & drop original sin cambios
- Posicionamiento de cartas intacto
- Interacciones familiares para el jugador
- Sistema de HandManager estable

### **🆕 Nuevas funcionalidades:**
- Cartas de asistencia con efectos acumulables
- Botones de gatillo para acciones principales
- Sistema estratégico de selección múltiple
- Efectos visuales de selección

### **🎮 Experiencia de Juego:**
- **Familiar**: Mismo drag & drop que antes
- **Estratégico**: Selección de cartas de asistencia
- **Flexible**: Botones para diferentes acciones
- **Visual**: Feedback claro de selección

---

## ⚠️ **Notas Importantes**

1. **Drag & Drop Original**: Funciona exactamente igual que antes
2. **Posicionamiento**: Las cartas mantienen su posición en la mano
3. **Cartas de Asistencia**: No terminan el turno al ser arrastradas
4. **Botones de Gatillo**: Solo aparecen después de confirmar selección
5. **Efectos Acumulables**: Se aplican automáticamente al gatillo

---

## 🎮 **Flujo de Ejemplo**

1. **Inicio**: Robar 4 cartas de asistencia
2. **Selección**: Arrastrar 2 cartas al enemigo
3. **Confirmación**: Hacer clic en "Confirmar Selección"
4. **Gatillo**: Hacer clic en botón "ATAQUE"
5. **Resultado**: Ataque con efecto duplicado (2 cartas × 2.0 = 4x daño)
6. **Fin**: Descartar cartas y terminar turno

¡El sistema mantiene la familiaridad del drag & drop original mientras agrega la nueva mecánica de cartas de asistencia y botones de gatillo!
