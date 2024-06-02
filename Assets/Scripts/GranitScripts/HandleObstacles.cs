using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class HandleObstacles : MonoBehaviourPunCallbacks
{
    PlayerManager playerManager;
    //public Text testText;


    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Grass")
        {
            SpriteRenderer spriteOpacity = other.gameObject.GetComponent<SpriteRenderer>();
            Color spriteColor = spriteOpacity.color;
            spriteColor.a = 1f;
            spriteOpacity.color = spriteColor;

            if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                playerManager.movePixelValue = 0.005f;
            }
            else
            {
                playerManager.movePixelValue = 0.5f;
            }            
        }
        if (other.tag == "Thorn")
        {
            SpriteRenderer spriteOpacity = other.gameObject.GetComponent<SpriteRenderer>();
            Color spriteColor = spriteOpacity.color;
            spriteColor.a = 1f;
            spriteOpacity.color = spriteColor;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                playerManager.movePixelValue = 0.005f;
            }
            else
            {
                playerManager.movePixelValue = 0.5f;
            }
        }
        if (other.tag == "Pit")
        {
            SpriteRenderer spriteOpacity = other.gameObject.GetComponent<SpriteRenderer>();
            Color spriteColor = spriteOpacity.color;
            spriteColor.a = 1f;
            spriteOpacity.color = spriteColor;
            playerManager.movePixelValue = 0f;
            StartCoroutine(PitWait());
        }        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Grass")
        {
            SpriteRenderer spriteOpacity = other.gameObject.GetComponent<SpriteRenderer>();
            Color spriteColor = spriteOpacity.color;
            spriteColor.a = 0.2f;
            spriteOpacity.color = spriteColor;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                playerManager.movePixelValue = 0.01f;
            }
            else
            {
                playerManager.movePixelValue = 1f;
            }
        }
        if (other.tag == "Thorn")
        {
            SpriteRenderer spriteOpacity = other.gameObject.GetComponent<SpriteRenderer>();
            Color spriteColor = spriteOpacity.color;
            spriteColor.a = 0.2f;
            spriteOpacity.color = spriteColor;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                playerManager.movePixelValue = 0.01f;
            }
            else
            {
                playerManager.movePixelValue = 1f;
            }
        }

        if (other.tag == "Pit")
        {
            SpriteRenderer spriteOpacity = other.gameObject.GetComponent<SpriteRenderer>();
            Color spriteColor = spriteOpacity.color;
            spriteColor.a = 0.2f;
            spriteOpacity.color = spriteColor;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                playerManager.movePixelValue = 0.01f;
            }
            else
            {
                playerManager.movePixelValue = 1f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(playerManager.jobIndex == 1 && photonView.IsMine)
        {
            //if (collision.transform.tag == "WallHor" || collision.transform.tag == "WallVer" || collision.transform.tag == "WallJoint")
            //{
            //    if (collision.gameObject.GetComponent<WallInfo>().wallHealth > 0)
            //    {
            //        collision.gameObject.GetComponent<WallInfo>().wallHealth--;

            //        if (collision.gameObject.GetComponent<WallInfo>().wallHealth == 0)
            //        {
            //            collision.gameObject.SetActive(false);
            //            collision.gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);

            //            if (collision.transform.tag == "WallHor")
            //            {
            //                GameMapGenerator.instance.removedInfo.positionBricksHor.Add(collision.gameObject.GetComponent<WallInfo>().wallCoordinate);
            //            }
            //            else if (collision.transform.tag == "WallVer")
            //            {
            //                GameMapGenerator.instance.removedInfo.positionBricksVer.Add(collision.gameObject.GetComponent<WallInfo>().wallCoordinate);
            //            }
            //            else
            //            {
            //                GameMapGenerator.instance.removedInfo.positionBricksJoint.Add(collision.gameObject.GetComponent<WallInfo>().wallCoordinate);
            //            }
            //        }
            //    }
            //}

            switch (collision.transform.tag)
            {
                case "WallHor":
                    if (collision.gameObject.GetComponent<WallInfo>().wallHealth > 0)
                    {
                        collision.gameObject.GetComponent<WallInfo>().wallHealth--;

                        if (collision.gameObject.GetComponent<WallInfo>().wallHealth == 0)
                        {
                            collision.gameObject.SetActive(false);
                            collision.gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);
                            SyncWallDestroy(new object[] { collision.gameObject.GetComponent<WallInfo>().wallCoordinate, 0 });
                        }
                    }
                    break;
                case "WallVer":
                    if (collision.gameObject.GetComponent<WallInfo>().wallHealth > 0)
                    {
                        collision.gameObject.GetComponent<WallInfo>().wallHealth--;

                        if (collision.gameObject.GetComponent<WallInfo>().wallHealth == 0)
                        {
                            collision.gameObject.SetActive(false);
                            collision.gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);
                            SyncWallDestroy(new object[] { collision.gameObject.GetComponent<WallInfo>().wallCoordinate, 1 });
                        }
                    }
                    break;
                case "WallJoint":
                    if (collision.gameObject.GetComponent<WallInfo>().wallHealth > 0)
                    {
                        collision.gameObject.GetComponent<WallInfo>().wallHealth--;

                        if (collision.gameObject.GetComponent<WallInfo>().wallHealth == 0)
                        {
                            collision.gameObject.SetActive(false);
                            collision.gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);
                            SyncWallDestroy(new object[] { collision.gameObject.GetComponent<WallInfo>().wallCoordinate, 2 });
                        }
                    }
                    break;
            }
        }                
    }

    private void OnCollisionExit(Collision collision)
    {
        
    }

    private IEnumerator PitWait()
    {
        yield return new WaitForSeconds(3f);

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            playerManager.movePixelValue = 0.01f;
        }
        else
        {
            playerManager.movePixelValue = 1f;
        }
    }

    public void SyncWallDestroy(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(5, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
