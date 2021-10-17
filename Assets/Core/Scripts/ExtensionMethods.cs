using UnityEngine;

namespace ExtensionMethods
{
    public static class EcsConversionExtension
    {
     
        public static  Unity.Physics.Ray FromEngineRay(UnityEngine.Ray engineRay)
        {
            return new Unity.Physics.Ray {Origin = engineRay.origin, Displacement = engineRay.direction};
        }      
        public static  UnityEngine.Ray ToEngineRay( this  Unity.Physics.Ray physicsRay)
        {
           return new UnityEngine.Ray {direction = physicsRay.Displacement, origin = physicsRay.Origin};
        }     
    }
}