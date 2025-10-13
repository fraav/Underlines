# Sistema Integrado de Cartas de Asistencia - Guía de Configuración

## 🎯 **Descripción del Sistema**
El sistema ha sido completamente integrado en los scripts existentes. Las cartas de asistencia ahora son cartas normales con `CardType.Assistance`, manteniendo el drag and drop existente y todos los sistemas actuales.

## 📋 **Configuración Paso a Paso**

### **1. Crear la Carta de Asistencia**

#### **Paso 1.1: Usar el Creador Automático**
```
1. En Unity, crear GameObject vacío → "CardCreator"
2. Agregar script: CreateDoubleEffectCard
3. Click derecho en el script → "Create Double Effect Assistance Card"
4. Se creará automáticamente: Assets/Cards_SO/Assistance/DoubleEffectCard.asset
```

#### **Paso 1.2: Configuración de la Carta**
La carta se crea con estos valores:
- **Card Name**: "Duplicar Efecto"
- **Card Type**: Assistance
- **Assistance Type**: DoubleEffect
- **Effect Multiplier**: 2.0
- **Description**: "Duplica el efecto del gatillo seleccionado..."

### **2. Configurar el GameManager**

#### **Paso 2.1: Asignar la Carta**
```
En GameManager Inspector:
- All Cards:
  └── Size: [Agregar +1 al tamaño actual]
  └── Element [Nuevo]: DoubleEffectCard
```

#### **Paso 2.2: Configurar Botones de Gatillo**
```
En GameManager Inspector:
- Trigger Buttons Panel → [Crear Panel con botones]
- Attack Trigger Button → [Botón Ataque]
- Block Trigger Button → [Botón Bloqueo]
- Heal Trigger Button → [Botón Cura]
```

### **3. Crear los Botones de Gatillo**

#### **Paso 3.1: Crear Panel de Botones**
```
1. Crear Panel → "TriggerButtonsPanel"
2. Configurar Layout: Horizontal Layout Group
3. Posición: Centro superior de la pantalla
4. Crear 3 botones hijos:
   ├── AttackTriggerButton (Texto: "ATAQUE", Color: Rojo)
   ├── BlockTriggerButton (Texto: "BLOQUEO", Color: Azul)
   └── HealTriggerButton (Texto: "CURA", Color: Verde)
```

#### **Paso 3.2: Configurar Eventos de Botones**
```
AttackTriggerButton:
- OnClick → GameManager.OnAttackTriggerClicked()

BlockTriggerButton:
- OnClick → GameManager.OnBlockTriggerClicked()

HealTriggerButton:
- OnClick → GameManager.OnHealTriggerClicked()
```

### **4. Configurar HandManager**

#### **Paso 4.1: Agregar Botón de Confirmación**
```
1. Crear Button → "ConfirmButton"
2. Texto: "Confirmar Selección"
3. Posición: Debajo de las cartas
4. En HandManager Inspector:
   - Confirm Button → ConfirmButton
```

#### **Paso 4.2: Configurar Evento**
```
ConfirmButton:
- OnClick → HandManager.OnConfirmButtonClicked()
```

### **5. Configurar Card Prefab**

#### **Paso 5.1: Verificar Prefab Existente**
El prefab de carta existente ya funciona, solo asegúrate de que tenga:
- **CardDisplay** script
- **Card** script (para drag and drop)
- **Button** component
- **Image** components para iconos

#### **Paso 5.2: El Prefab Ya Está Listo**
No necesitas crear un prefab separado. El sistema existente maneja tanto cartas normales como cartas de asistencia.

---

## 🎮 **Flujo del Nuevo Sistema**

### **Fase 1: Inicio del Turno**
1. Se roban 4 cartas (pueden ser normales o de asistencia)
2. Aparece el botón "Confirmar Selección"
3. Las cartas de asistencia se pueden seleccionar con drag & drop

### **Fase 2: Selección de Cartas de Asistencia**
1. **Arrastrar cartas de asistencia** para seleccionarlas (0-4 cartas)
2. **Feedback visual**: Las cartas seleccionadas se resaltan
3. **Hacer clic en "Confirmar Selección"** cuando termines

### **Fase 3: Botones de Gatillo**
1. Aparecen 3 botones: ATAQUE, BLOQUEO, CURA
2. **Elegir uno** según la estrategia
3. Se ejecuta la acción + efectos acumulables de las cartas de asistencia

### **Fase 4: Fin de Turno**
1. Se descartan todas las cartas restantes
2. Termina el turno del jugador
3. Inicia el turno del enemigo

---

## 🔧 **Diferencias Clave del Sistema Integrado**

### **✅ Lo que se mantiene igual:**
- **Drag & Drop**: Funciona exactamente igual
- **HandManager**: Mismo sistema de cartas
- **CardDisplay**: Mismo sistema de visualización
- **Card.cs**: Mismo sistema de interacción

### **🆕 Lo que cambia:**
- **CardData**: Ahora tiene campos para cartas de asistencia
- **GameManager**: Nuevos estados y métodos integrados
- **Flujo de turno**: Asistencia → Gatillo → Fin turno

### **🎯 Ventajas del Sistema Integrado:**
1. **Sin scripts duplicados**: Todo en los scripts existentes
2. **Drag & Drop nativo**: Usa el sistema actual
3. **Compatibilidad total**: Funciona con el sistema anterior
4. **Fácil configuración**: Solo agregar cartas y botones

---

## 📁 **Estructura de Archivos Final**

```
Assets/
├── Cards_SO/
│   └── Assistance/
│       └── DoubleEffectCard.asset
├── Scripts_Card/
│   ├── CardData.cs (modificado)
│   ├── GameManager.cs (modificado)
│   ├── HandManager.cs (modificado)
│   ├── CardDisplay.cs (modificado)
│   ├── Card.cs (modificado)
│   └── CreateDoubleEffectCard.cs
└── CartaPrefab/
    └── [Prefab existente funciona igual]
```

---

## 🚀 **Comandos de Testing**

```csharp
// Verificar estado del sistema
Debug.Log($"Cartas en mano: {currentHand.Count}");
Debug.Log($"Cartas seleccionadas: {selectedAssistanceCards.Count}");
Debug.Log($"Estado actual: {currentTurn}");
```

---

## ⚠️ **Notas Importantes**

1. **Solo necesitas crear cartas de asistencia** - el resto del sistema ya funciona
2. **El drag & drop es el mismo** - no hay cambios en la interacción
3. **Los botones de gatillo son opcionales** - puedes usar solo cartas de asistencia
4. **Compatible con cartas existentes** - puedes mezclar cartas normales y de asistencia

¡El sistema está completamente integrado y listo para usar!
