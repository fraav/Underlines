# Guía de Creación de Cartas de Asistencia

## 🎯 **Cómo Crear Cartas de Asistencia**

### **Método Estándar de Unity**

#### **Paso 1: Crear la Carta**
```
1. En Project Window → Click derecho
2. Create → Card Game → Card
3. Nombrar el archivo (ej: "DuplicarEfecto")
```

#### **Paso 2: Configurar la Carta de Asistencia**

**Configuración Básica:**
- **Card Name**: "Duplicar Efecto"
- **Card Type**: Assistance ⭐
- **Description**: "Duplica el efecto del gatillo seleccionado"

**Configuración de Asistencia:**
- **Assistance Type**: DoubleEffect
- **Effect Multiplier**: 2.0
- **Card Color**: Magenta
- **Text Color**: White

**Configuración Visual:**
- **Icon**: Asignar sprite del icono
- **Shop Icon**: Asignar sprite para tienda

#### **Paso 3: Agregar al GameManager**
```
1. Seleccionar GameManager en la escena
2. En Inspector → All Cards
3. Size: Aumentar en +1
4. Element [Nuevo]: Arrastrar la carta creada
```

---

## 📋 **Tipos de Cartas de Asistencia Disponibles**

### **1. Duplicar Efecto**
- **Assistance Type**: DoubleEffect
- **Effect Multiplier**: 2.0
- **Descripción**: Duplica el efecto del gatillo
- **Uso**: Si tienes 2 cartas, el efecto se duplica 4 veces

### **2. Daño Extra**
- **Assistance Type**: ExtraDamage
- **Effect Multiplier**: 1.5
- **Descripción**: Aumenta el daño del gatillo de ataque

### **3. Bloqueo Extra**
- **Assistance Type**: ExtraBlock
- **Effect Multiplier**: 1.5
- **Descripción**: Aumenta la protección del gatillo de bloqueo

### **4. Cura Extra**
- **Assistance Type**: ExtraHeal
- **Effect Multiplier**: 1.5
- **Descripción**: Aumenta la curación del gatillo de cura

---

## 🎮 **Ejemplos de Configuración**

### **Ejemplo 1: Carta "Duplicar Efecto"**
```
Card Name: "Duplicar Efecto"
Card Type: Assistance
Description: "Duplica el efecto del gatillo seleccionado"
Assistance Type: DoubleEffect
Effect Multiplier: 2.0
Card Color: Magenta
Text Color: White
Base Value: 0
```

### **Ejemplo 2: Carta "Ataque Fuerte"**
```
Card Name: "Ataque Fuerte"
Card Type: Assistance
Description: "Aumenta el daño del ataque"
Assistance Type: ExtraDamage
Effect Multiplier: 1.5
Card Color: Red
Text Color: White
Base Value: 0
```

### **Ejemplo 3: Carta "Protección Mejorada"**
```
Card Name: "Protección Mejorada"
Card Type: Assistance
Description: "Mejora el bloqueo"
Assistance Type: ExtraBlock
Effect Multiplier: 1.3
Card Color: Blue
Text Color: White
Base Value: 0
```

---

## ⚙️ **Configuración del GameManager**

### **Paso 1: Agregar Cartas**
```
En GameManager Inspector:
- All Cards:
  ├── [Cartas existentes]
  ├── DuplicarEfecto
  ├── AtaqueFuerte
  └── ProteccionMejorada
```

### **Paso 2: Verificar Configuración**
```
Asegúrate de que también estén configurados:
- Trigger Buttons Panel
- Attack Trigger Button
- Block Trigger Button
- Heal Trigger Button
```

---

## 🔧 **Notas Importantes**

### **✅ Lo que funciona automáticamente:**
- Las cartas de asistencia aparecen en la mano normal
- El drag & drop funciona igual que las cartas normales
- Se pueden seleccionar 0-4 cartas de asistencia
- Los efectos se acumulan automáticamente

### **🎯 Tips de Creación:**
1. **Effect Multiplier**: Usa valores como 1.5, 2.0, 3.0 para efectos balanceados
2. **Colores**: Usa colores distintivos para cada tipo de asistencia
3. **Descripciones**: Sé claro sobre qué hace cada carta
4. **Nombres**: Usa nombres descriptivos y cortos

### **⚠️ Limitaciones:**
- Solo se pueden seleccionar hasta 4 cartas de asistencia por turno
- Los efectos solo se aplican al gatillo seleccionado
- Las cartas de asistencia no terminan el turno por sí solas

---

## 🚀 **Testing**

Para probar las cartas:
1. Crear una carta de asistencia
2. Agregarla al GameManager
3. Iniciar una batalla
4. Verificar que aparezca en la mano
5. Arrastrarla para seleccionarla
6. Confirmar selección
7. Elegir un gatillo
8. Verificar que el efecto se aplique

¡Las cartas de asistencia ahora se crean como cartas normales usando el menú estándar de Unity!
