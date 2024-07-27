using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

public class NavMesh : MonoBehaviour
{
   public static NavMesh Instance { get; private set; }
   private void Awake()
   {
      // If there is an instance, and it's not me, delete myself.
      if (Instance != null && Instance != this)
      {
         Destroy(this);
      }
      else
      {
         Instance = this;
      }
   }
   public bool update = false;
   public NavMeshSurface[] navMeshSurfaces;
   public NavMeshSurface navMeshSurface;
   int childAmount = 0;
   void Start()
   {
      navMeshSurface = GetComponent<NavMeshSurface>();
      StartCoroutine(updateperiod());
   }

   IEnumerator updateperiod()
   {
      while (true)
      {
         if (update) { rebuild(); update = false; }
         yield return new WaitForSeconds(0.5f);
      }
   }
   NavMeshSurface temp;
   void addSurfaces()
   {
      for (int i = 0; i < transform.childCount; i++)
      {
         if (!transform.GetChild(i).gameObject.TryGetComponent(out temp))
         {
            transform.GetChild(i).gameObject.AddComponent<NavMeshSurface>();
         }

      }
      navMeshSurfaces = transform.GetComponentsInChildren<NavMeshSurface>();
   }
   bool built = false;
   IEnumerator wait()
   {

      float timestarted = Time.time;
      asyncOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
      while (!asyncOperation.isDone)
      {


         //Debug.Log(asyncOperation.progress);
         yield return null;
      }

      
   }
   AsyncOperation asyncOperation;
   public void rebuild()
   {
      if (built)
      {
         StartCoroutine(wait());
      }
      else
      {
         navMeshSurface.BuildNavMesh();
         built = true;
      }


      return;
      if (childAmount != transform.childCount)
      {
         childAmount = transform.childCount;
         addSurfaces();
      }
      foreach (var s in navMeshSurfaces)
      {
         GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
      }
   }
}
