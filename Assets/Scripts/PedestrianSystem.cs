using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianSystem : MonoBehaviour
{
    [SerializeField] private List<Color> pedestrianColors = new List<Color>();
    [SerializeField] private GameObject pedestrianParentObject = null;
    [SerializeField] private GameObject pedestrianPrefab = null;
    [SerializeField] private HeatmapHandler heatmapHandler = null;
    [SerializeField] private uint initialPoolSize = 100;
    [SerializeField] private int densityRecalculationFrequency = 10;
    LinkedList<PedestrianPoolObject> pedestrianPool = new LinkedList<PedestrianPoolObject>();
    List<PedestrianEntity> pedestrianEntities = new List<PedestrianEntity>();
    [SerializeField] GameState gameState = null;
    List<Material> ColorStorage = new List<Material>();
    List<GameObject> trajectories = new List<GameObject>();

    private void Awake()
    {
        gameState.isPlayingEvent += OnIsPlayingChanged;
        gameState.trajectoryModeEvent += OnShowTrajectoriesChanged;

        for (int i = 0; i < initialPoolSize; ++i) {
            CreatePoolObject();
        }
    }

    private void OnIsPlayingChanged(bool isPlaying)
    {
        foreach (var entry in pedestrianEntities)
        {
            if(isPlaying)
                entry.poolObject.componentAnimation.Play();
            else
                entry.poolObject.componentAnimation.Stop();
        }
    }

    public void AddPedestrianEntity(PedestrianEntity entity) {
        if (pedestrianPool.Count == 0) CreatePoolObject();

        PedestrianPoolObject poolObject = pedestrianPool.Last.Value;
        pedestrianPool.RemoveLast();
        entity.poolObject = poolObject;
        pedestrianEntities.Add(entity);

        if (gameState.IsPlaying)
            poolObject.componentAnimation.Play();
        else
            poolObject.componentAnimation.Stop();
        poolObject.obj.SetActive(true);
    }

    public void ClearPedestrianEntities() {
        foreach (var entry in pedestrianEntities) {
            entry.poolObject.obj.SetActive(false);
            entry.poolObject.componentAnimation.Stop();
            pedestrianPool.AddLast(entry.poolObject);
        }
        pedestrianEntities.Clear();
    }
    
    private void Update()
    {
        if (gameState.IsPlaying)
        {
            try
            {
                gameState.CurrentTime = (gameState.CurrentTime + (float)Time.deltaTime) % gameState.TotalTime;
            }
            catch (DivideByZeroException)
            {
                gameState.CurrentTime = 0;
            }
        }

        float currentTime = gameState.CurrentTime;
        TileColoringMode tileColoringMode = gameState.PawnColoringMode;
        bool recalculateDensity = (Time.frameCount % densityRecalculationFrequency == 0);

        foreach (var entity in pedestrianEntities)
        {
            PedestrianPoolObject poolObject = entity.poolObject;

            int positionIndex = entity.getIndexOfNewTime(currentTime);

            if (positionIndex < entity.positions.Count - 1 && positionIndex > -1)
            {
                poolObject.pedestrianRenderer.enabled = true;
                PedestrianPosition pos = entity.positions[positionIndex];
                PedestrianPosition pos2 = entity.positions[positionIndex + 1];
                Vector3 start = new Vector3(pos.getX(), pos.getZ(), pos.getY());
                Vector3 target = new Vector3(pos2.getX(), pos2.getZ(), pos2.getY());

                float movement_percentage = currentTime - pos.getTime();
                Vector3 newPosition = Vector3.Lerp(start, target, movement_percentage);

                // to keep pedestrians upright change in the Z Axis is excluded from rotation calculation
                Vector3 relativePos = new Vector3(pos2.getX(), 0, pos2.getY()) - new Vector3(pos.getX(), 0, pos.getY());
                float speed = relativePos.magnitude;

                poolObject.animationWalking.speed = speed;

                if (start != target) poolObject.obj.transform.localRotation = Quaternion.LookRotation(relativePos);

                //Tile coloring
                if (tileColoringMode != TileColoringMode.TileColoringNone)
                {


                    if (tileColoringMode == TileColoringMode.TileColoringSpeed)
                    {
                        poolObject.tileRenderer.enabled = true;
                        //1.5 as average maximum walking speed when not hindered
                        poolObject.tileRenderer.material.color = ColorHelper.ColorForSpeed(speed, 1.5f);
                    }
                    else if (tileColoringMode == TileColoringMode.TileColoringDensity)
                    {
                        if (recalculateDensity)
                        {
                            float density = getDensity(entity);

                            if (density >= gameState.DensityThreshold)
                            {
                                poolObject.tileRenderer.enabled = true;
                                poolObject.tileRenderer.material.color = ColorHelper.ColorForDensity(density);
                            }
                            else
                            {
                                poolObject.tileRenderer.enabled = false;
                            }

                        }

                    }
                }
                else
                {
                    poolObject.tileRenderer.enabled = false;
                }

                poolObject.obj.transform.localPosition = newPosition;
                heatmapHandler.AddPedestrian(newPosition); //ED: new //TODO:


            }
            else
            {
                poolObject.pedestrianRenderer.enabled = false;
                poolObject.tileRenderer.enabled = false;
            }
            entity.poolObject.trajectoryRenderer.widthMultiplier = entity.poolObject.obj.transform.lossyScale.x;
        }
    }

    private float getDensity(PedestrianEntity current)
    {
        int nearbys = 0;
        float radius = 2.0f;
        foreach (var entity in pedestrianEntities)
        {

            if (current.id != entity.id) {
                if (Vector3.Distance(current.poolObject.obj.transform.localPosition,
                    entity.poolObject.obj.transform.localPosition) < radius) {
                    nearbys++;
                }
            }
        }
        float density = nearbys / (radius * radius * Mathf.PI);
        return density;
    }

    private void CreatePoolObject() {
        GameObject gameObject = (GameObject)Instantiate(Resources.Load(pedestrianPrefab.name));
        gameObject.transform.parent = pedestrianParentObject.transform;
        gameObject.SetActive(false);
        PedestrianPoolObject poolObject = new PedestrianPoolObject(gameObject
            , new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), 
            pedestrianParentObject.transform);
        pedestrianPool.AddLast(poolObject);
    }

    private void OnShowTrajectoriesChanged(bool show)
    {
        if (show)
            foreach (var entry in pedestrianEntities)
            {
                List<Vector3> points = new List<Vector3>();
                
                for (int i = 0; i < entry.positions.Count - 1; i++)
                {
                    PedestrianPosition a = (PedestrianPosition)entry.positions[i];
                    points.Add(new Vector3(a.getX(), a.getZ() + 0.01f, a.getY()));
                }
                entry.poolObject.trajectoryRenderer.positionCount = points.Count;
                entry.poolObject.trajectoryRenderer.SetPositions(points.ToArray());
                entry.poolObject.trajectoryRenderer.enabled = true;
            }
        else
            foreach (var entry in pedestrianEntities)
            {
                entry.poolObject.trajectoryRenderer.enabled = false;
            }
    }
}
