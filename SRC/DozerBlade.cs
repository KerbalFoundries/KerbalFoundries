using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    public class DozerBlade :PartModule
    {
        List <WheelCollider> _wcList = new List<WheelCollider>();
        Transform _joint;
        bool isConfigured;
        GameObject _rockPrefab;

        List<Transform> _spawnPosition = new List<Transform>();

        [KSPField]
        public float spawnChance;
        [KSPField]
        public float maxDistance;
        [KSPField]
        public float bladeForce = 1;

        [KSPField]
        public float resistance;
        [KSPField]
        public string jointName;
        [KSPField]
        public float rotationSpeed = 1f;
        [KSPField]
        public Vector3 rotationAxis;
        [KSPField]
        public string spawnPoint = "SP";
        [KSPField]
        public string spawnObject;
        [KSPField]
        public float scale = 1f;
        [KSPField(guiName = "Enable Rocks",guiActive = true), UI_Toggle(enabledText = "Enabled", disabledText = "disabled")]
        public bool rocksEnabled;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            _joint = this.part.transform.Search(jointName);

            if(HighLogic.LoadedSceneIsFlight)
            {
                foreach(WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    _wcList.Add(wc);
                    wc.enabled = true;
                    Debug.LogWarning("Dozer Collider enabled");
                } 
            }
            RockSetup();
            isConfigured = true;
                Debug.LogWarning("Dozer configured");
        }

        public void Update()
        {
            if (!isConfigured)
                return;
            float rotation = 0; 
            if (!FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS])
            {
                rotation = this.vessel.ctrlState.Z * rotationSpeed * Time.deltaTime;
            }

            _joint.transform.Rotate(rotationAxis, rotation);



            for (int i = 0; i < _wcList.Count(); i++)
            {
                _wcList[i].brakeTorque = resistance;
                WheelHit hit; //set this up to grab sollider raycast info
                bool grounded = _wcList[i].GetGroundHit(out hit); //set up to pass out wheelhit coordinates

                //float tempLastFrameTraverse = lastFrameTraverse; //we need the value, but will over-write shortly. Store it here.
                //(-_wcList[i].transform.InverseTransformPoint(hit.point).y < 0.5f) && 
                if ( (hit.sidewaysSlip > 0.1f || hit.sidewaysSlip < -0.1f)) //is it on the ground
                {
                    if ((-hit.sidewaysSlip * UnityEngine.Random.Range(0, 100) > spawnChance) && rocksEnabled)
                    {
                        SpawnRock(i);
                    }
                    this.part.rigidbody.AddForceAtPosition(-_wcList[i].transform.up * bladeForce, _wcList[i].transform.position);
                }
                //print("slip  " + hit.sidewaysSlip + " " + i);
                //print(-_wcList[i].transform.InverseTransformPoint(hit.point).y);
            }
        }

        public void RockSetup()
        {
            for (int i = 0; i < _wcList.Count(); i++)
            {
                _spawnPosition.Add(this.part.FindModelTransform(spawnPoint + _wcList[i].name.GetLast(3)));

                    Debug.LogWarning(spawnPoint + _wcList[i].name.GetLast(3));

                _rockPrefab = GameDatabase.Instance.GetModel(spawnObject);
                _rockPrefab.SetActive(true);
                    Debug.LogWarning("Rock scale is" + _rockPrefab.transform.localScale);
                _rockPrefab.transform.localScale = new Vector3(scale, scale, scale); 
                    Debug.LogWarning("Rock scale is" + _rockPrefab.transform.localScale);

                var rb = _rockPrefab.AddComponent<Rigidbody>();
                rb.mass = 0.01f;
                rb.angularDrag = 5f;

                _rockPrefab.SetActive(false);
            }
        }

        public void SpawnRock(int i)
        {
            var rock = (GameObject)Instantiate(_rockPrefab, _spawnPosition[i].transform.position, new Quaternion(0, 0, 0, 0));
            
                //Debug.LogWarning("Object instantiated");
            var physicRock = rock.gameObject.AddComponent<physicalObject>();
            physicRock.maxDistance = maxDistance;
                //Debug.LogWarning("rock set");
            rock.gameObject.SetActive(true);
            var randomScale = UnityEngine.Random.Range(0.5f, 1.2f);
            GameObject rockCollider = rock.GetComponentInChildren<Collider>().gameObject;
            if (scale < 0.8f)
            {
                Destroy(rockCollider);
            }
            rock.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            rock.rigidbody.velocity = this.part.vessel.srf_velocity;
        }
    }
}
