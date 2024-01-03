// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2019-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

public enum SceneMovementMode
{
    Fly,
    Pivot
}

public class ZIPoseHarness : MonoBehaviour
{
    public IPoseDriver driver;

    private Vector2 currentMousePos_ = Vector2.zero;

    void Update()
    {
        if (driver != null)
        {
            if (Keyboard.current[Key.W].isPressed)
            {
                driver.Translate(transform.forward * driver.MovementSpeed);
            }
            if (Keyboard.current[Key.S].isPressed)
            {
                driver.Translate(transform.forward * -driver.MovementSpeed);
            }
            if (Keyboard.current[Key.A].isPressed)
            {
                driver.Translate(transform.right * -driver.MovementSpeed);
            }
            if (Keyboard.current[Key.D].isPressed)
            {
                driver.Translate(transform.right * driver.MovementSpeed);
            }
            if (Keyboard.current[Key.Q].isPressed)
            {
                driver.Translate(transform.up * -driver.MovementSpeed);
            }
            if (Keyboard.current[Key.E].isPressed)
            {
                driver.Translate(transform.up * driver.MovementSpeed);
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                // initialize current mouse press on first update
                currentMousePos_ = Mouse.current.position.ReadValue();
            }
            else if (Mouse.current.rightButton.isPressed)
            {
                // handle rotation via mouse delta
                Vector2 newMousePos = Mouse.current.position.ReadValue();
                Vector2 mouseDelta = newMousePos - currentMousePos_;
                if (mouseDelta.x != 0)
                {
                    float yawDelta = mouseDelta.x * driver.RotationSpeed;

                    if (driver.MovementMode == SceneMovementMode.Fly)
                    {
                        float dotUp = Vector3.Dot(transform.right, Vector3.up);
                        if (dotUp < 0.01f && dotUp > -0.01f)
                        {
                            driver.Rotate(Quaternion.Euler(0.0f, yawDelta, 0.0f));
                        }
                        else
                        {
                            driver.Rotate(Quaternion.AngleAxis(yawDelta, transform.up).normalized);
                        }
                    }
                    else if (driver.MovementMode == SceneMovementMode.Pivot)
                    {
                        driver.Rotate(Quaternion.Euler(0.0f, yawDelta, 0.0f));
                    }
                }
                if (mouseDelta.y != 0)
                {
                    float pitchDelta = -mouseDelta.y * driver.RotationSpeed;
                    driver.Rotate(Quaternion.AngleAxis(pitchDelta, transform.right).normalized);
                }
                currentMousePos_ = newMousePos;
            }
        }
    }

    public void SetDriver(IPoseDriver driver)
    {
        this.driver = driver;
        driver.Initialize();
    }
}
#endif
