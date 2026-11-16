

using System.Collections;
using AstarRoutine.Astar.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MoveSphere:MonoBehaviour
{
    private EntityManager entityManager;
    private Entity sphereEntity;

    public float moveSpeed;
    private void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return new WaitForSeconds(2f);
        if (entityManager == default)
        {
            entityManager =  World.DefaultGameObjectInjectionWorld.EntityManager;
        }
        var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<ATestSphere>());
        if (query.CalculateEntityCount() == 1)
        {
            sphereEntity = query.GetSingletonEntity();
        }
        else
        {
            Debug.LogWarning($"ATestSphere 实体数量 = {query.CalculateEntityCount()}，不是1个");
        }
        query.Dispose();
    }

    private void Update()
    {
        entityManager =  World.DefaultGameObjectInjectionWorld.EntityManager;
        if (entityManager.Exists(sphereEntity))
        {
            LocalTransform lt = entityManager.GetComponentData<LocalTransform>(sphereEntity);

            if (Input.GetKey(KeyCode.W))
            {
                lt.Position.z += moveSpeed*Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                lt.Position.z -= moveSpeed*Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                lt.Position.x -= moveSpeed*Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                lt.Position.x += moveSpeed*Time.deltaTime;
            }
            entityManager.SetComponentData(sphereEntity, lt);
        }
    }
}