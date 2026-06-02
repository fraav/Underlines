using UnityEngine;

public class KeepBetweenScenes : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Si está activado, el objeto se mantendrá entre escenas")]
    public bool keepBetweenScenes = true;
    
    [Tooltip("Si está activado, destruirá duplicados automáticamente")]
    public bool destroyDuplicates = true;
    
    void Awake()
    {
        if (!keepBetweenScenes) return;
        
        if (destroyDuplicates)
        {
            // Buscar todos los objetos con el mismo nombre
            KeepBetweenScenes[] existingObjects = FindObjectsOfType<KeepBetweenScenes>();
            
            foreach (KeepBetweenScenes obj in existingObjects)
            {
                if (obj != this && obj.gameObject.name == gameObject.name)
                {
                    // Destruir duplicados
                    Destroy(gameObject);
                    return;
                }
            }
        }
        
        // Mantener este objeto entre escenas
        DontDestroyOnLoad(gameObject);
    }
}