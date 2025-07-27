# No Sign From Zenith
![No Sign From Zenith Promo Art](https://github.com/AlessandroSimeoni/NoSignFromZenith/blob/main/Capsula%20Principale.png)  

Signs of communication from the Zenith spaceship suddenly ceased after an asteroid hit it.  
AL3X, a scout robot, is sent on Zenith to discover what happened.  
The astronauts were sent to bomb the sun with nuclear weapons to save humanity from the next glacial era, but the spacecraft’s AI F.R.E.E. and the robots around the base mysteriously shut down.

Overcome puzzles and logic challenges to explore the starship looking for answers.  

No Sign From Zenith is a first person puzzle game developed in Unity. It is the first 3D project made at the game academy.
It was developed in a month in a team of 13 people (6 designers, 2 programmers, 3 concept artists and 2 3D artists).
During the development of this project I started to improve my technical skills in the game development and began to learn how to communicate between departments.

### ***Watch the trailer [here](https://www.youtube.com/watch?v=iBSK3oFVLik).***
### ***Download the build [here](https://github.com/AlessandroSimeoni/NoSignFromZenith/releases/tag/NoSignFromZenith_Release).***

<a name="What-I-did"></a>
# What I did
In this project I worked on all the main features.  
The most important one is the gauntlet, the tool the player is holding ([here](https://github.com/AlessandroSimeoni/NoSignFromZenith/tree/main/Assets/Scripts/Character/GauntletActions) are all the scripts for this feature).  
It has two functions:
* [Item grab](#Item-grab)
* [Teleport](#Teleport)

<a name="Item-grab"></a>
## 1. Item grab
Some items can be grabbed by pointing the gauntlet on them and pressing the mouse input (it is inspired by the famous gravity gun of Half Life 2).  

The logic of item grab uses the unity's physics system.  
When grabbing an item a coroutine is called which sets up the rigidbody of the grabbed object and other variables.  
```
        /// <summary>
        /// Grab the object picked by the raycast
        /// </summary>
        /// <param name="hitInfo">the info of the raycast hit</param>
        private IEnumerator GrabObject(RaycastHit hitInfo)
        {
            requestMovement = false;
            currentObjectDistance = grabbedObjectDefaultDistance;

            grabbedObject = hitInfo.transform.gameObject.GetComponent<PickableObject>();
            grabbedObject.invokeRelease.AddListener(ReleaseObject);
            grabbedObjectRb = grabbedObject.GetComponent<Rigidbody>();

            // save object's options to later revert the changes
            SaveGrabbedObjectOptions();

            // change layer to avoid player collision
            grabbedObject.gameObject.layer = LayerMask.NameToLayer("GrabbedObject");

            // cancel velocity applied to rigidbody before grabbing
            grabbedObjectRb.velocity = Vector3.zero;

            // disable the use of gravity
            grabbedObjectRb.useGravity = false;

            // set the drag
            grabbedObjectRb.drag = objectDrag;

            isGrabbing = true;
            AudioPlayer.PlaySFX(grabSFX, transform.position, 0.0f);
            onGrabOrRelease.Invoke();

            // Wait for a little bit before setting up the object collision event.
            // The grabbed object uses the onCollisionStay method, which works great with the ReduceDistance mode,
            // but not with the ReleaseObject mode.
            // In this way the object will not be released at the moment of pick up.
            // (yield return null; was not enough)
            yield return new WaitForSeconds(0.1f);

            // check if the object has not already been released
            if (grabbedObject != null)
            {
                grabbedObject.onCollision.AddListener(HandleGrabbedObjectCollision);
                grabbedObject.GrabbedState(true);
            }
        }
```

The grabbed object is **not parented with the player or the gauntlet**, instead it follows a target point in space in front of the player.  
Once grabbed, the object began to move towards the target point by appliyng a force to its rigidbody.
When the player moves, the object moves away from the target point and when the distance from the target point is greater then a threshold, the force is applied again to reach the target position.  

```
        private void Update()
        {
            // do nothing if is not grabbing
            if (!isGrabbing)
                return;

            // calculate target position and current distance from target position
            targetPosition = mainCamera.transform.position + mainCamera.transform.forward * currentObjectDistance;
            distanceFromTargetPosition = targetPosition - grabbedObject.transform.position;

            // request movement if distance magnitude is greater than the offset
            if (distanceFromTargetPosition.sqrMagnitude > deadzoneMovementOffset)
                requestMovement = true;
        }

        private void FixedUpdate()
        {
            if (grabbedObject != null)
            {
                // align the grabbed object transform forward with the camera one
                grabbedObjectRb.MoveRotation(
                    Quaternion.Lerp(grabbedObjectRb.rotation,
                                    Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up),
                                    objectForwardRotationSpeed));

                if (requestMovement)
                    MoveGrabbedObject();
            }
        }

        private void MoveGrabbedObject()
        {
            Vector3 forceToApply = Vector3.ClampMagnitude(distanceFromTargetPosition * movementForce, movementForce);
            grabbedObjectRb.AddForce(forceToApply, ForceMode.Impulse);

            //cancel angular velocity
            grabbedObjectRb.angularVelocity = Vector3.zero;

            requestMovement = false;
        }
```
You can find the script for item grab [here](https://github.com/AlessandroSimeoni/NoSignFromZenith/blob/main/Assets/Scripts/Character/GauntletActions/SingleActions/PickUpObject.cs).  

The grabbed item can also be moved or thrown by the player.  

### 1.1 Item movement
The player can move the object back and forth using the mouse wheel.  
The logic for item movement simply updates the variable *currentObjectDistance* which, in turns, updates the target position of the grabbed object.  

Find the script [here](https://github.com/AlessandroSimeoni/NoSignFromZenith/blob/main/Assets/Scripts/Character/GauntletActions/SingleActions/MoveObject.cs).  

### 1.2 Item throwing
The player can throw the grabbed item.  
Even in this case the logic is pretty simple. A force is applied to the object and immediately after the item is released.  

```
        public override void PerformAction(GauntletAction dependantAction = null, float value = 0.0f)
        {
            PickUpObject pickUpAction = (PickUpObject) dependantAction;

            // throw the object
            Vector3 cameraForward = pickUpAction.mainCamera.transform.forward;
            pickUpAction.grabbedObjectRb.AddForce(cameraForward * throwingForce, ForceMode.Impulse);
            AudioPlayer.PlaySFX(pushSFX, transform.position, 0.0f);

            // release the object
            pickUpAction.ReleaseObject();
        }
```

Script [here](https://github.com/AlessandroSimeoni/NoSignFromZenith/blob/main/Assets/Scripts/Character/GauntletActions/SingleActions/ThrowObject.cs).

**[⬆ Back to Top](#What-I-did)**

<a name="Teleport"></a>
## 2. Teleport
The second function of the gauntlet is the teleport.  
Player can change the gauntlet mode with the press of the **R** key. When in the teleport mode, he can teleport towards some items/walls marked with violet color.  
When triggering the teleport action, a check is performed to verify if there is enough space to teleport the player, thus avoiding unwanted interpenetrations.  

```
        /// <summary>
        /// Check if there is enough space to teleport the player
        /// </summary>
        /// <param name="hit">the raycast hit info</param>
        /// <param name="surface">the type of surface selected for the teleport</param>
        /// <returns>true if there is enough space to teleport, false otherwise</returns>
        private bool CanTeleport(RaycastHit hit, TeleportSurface surface)
        {
            Ray sphereRay;

            // check the teleport area based on the type of surface pointed
            switch (surface)
            {
                case TeleportSurface.floor:
                    sphereRay = new Ray(hit.point +
                        hit.normal * (characterController.radius + characterController.skinWidth),
                        Vector3.up);
                    break;
                case TeleportSurface.wall:
                    sphereRay = new Ray(hit.point +
                        hit.normal * (characterController.radius + characterController.skinWidth) +
                        Vector3.up * characterController.radius,
                        Vector3.up);
                    break;
                default:
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TeleportPickableObject"))
                    {
                        Vector3 flattedNormal = new Vector3(hitInfo.normal.x, 0.0f, hitInfo.normal.z).normalized;

                        sphereRay = new Ray(hit.point +
                        flattedNormal * (characterController.radius + characterController.skinWidth + pickableObjectOffset) +
                        Vector3.up * pickableObjectOffset,
                        Vector3.up);
                    }
                    else
                        sphereRay = new Ray(hit.point +
                            hit.normal * (characterController.radius + characterController.skinWidth),
                            Vector3.down);
                    break;
            }

            // if there is space for the player then returns true
            return Physics.SphereCastAll(sphereRay,
                                         characterController.radius,
                                         characterController.height - 2 * characterController.radius,
                                         ~ignoreLayers  //ignore trigger areas
                                         ).Length == 0;
        }
```

This check is done with different input parameters based on the *normalOrientation* of the surface hit by the raycast.  

```
        public override void PerformAction(GauntletAction dependantAction = null, float value = 0)
        {
            canTeleport = false;
            ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            if (Physics.Raycast(ray, out hitInfo, teleportRange, teleportLayers))
            {
                // calculate the dot product to know the normal orientation
                float normalOrientation = Vector3.Dot(Vector3.up, hitInfo.normal);

                // check the normal orientation using an offset to avoid edge cases that can cause problems due to high precision of the dot product
                switch (normalOrientation)
                {
                    // normal points downwards
                    case < -normalOrientationOffset:
                        canTeleport = CanTeleport(hitInfo, TeleportSurface.ceiling);
                        // handle a specific case where the object pointed is a pickable object with the normal of the face points downwards
                        if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("TeleportPickableObject"))
                        {
                            Vector3 flattedNormal = new Vector3(hitInfo.normal.x, 0.0f, hitInfo.normal.z).normalized;
                            targetPosition = hitInfo.point + flattedNormal * (characterController.skinWidth + characterController.radius + pickableObjectOffset);
                        }
                        else
                            targetPosition = hitInfo.point + hitInfo.normal * (characterController.skinWidth + characterController.height);
                        break;
                    // normal points upwards
                    case > normalOrientationOffset:
                        canTeleport = CanTeleport(hitInfo, TeleportSurface.floor);
                        targetPosition = hitInfo.point + hitInfo.normal * characterController.skinWidth;
                        break;
                    // normal is parallel to the ground
                    default:
                        canTeleport = CanTeleport(hitInfo, TeleportSurface.wall);
                        targetPosition = hitInfo.point + hitInfo.normal * (characterController.radius + characterController.skinWidth);
                        break;
                }

                // teleport if possible
                if (canTeleport)
                {
                    characterMovement.Teleport(targetPosition);
                    AudioPlayer.PlaySFX(teleportSFX, targetPosition);
                }
            }
        }
```
Script [here](https://github.com/AlessandroSimeoni/NoSignFromZenith/blob/main/Assets/Scripts/Character/GauntletActions/SingleActions/Teleport.cs).  

**[⬆ Back to Top](#What-I-did)**
