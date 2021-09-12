using System;
using System.IO;
using System.Text;
using Core.Managers;
using Core.Saving;
using UnityEngine;

public class PlayerSavingManager : SavingManager
{
    public override void Save(SavingContext context)
    {
        PlayerIOContext ctx = ((PlayerIOContext) context);

        if (!Directory.Exists(GameManager.CurrentWorldPath))
            Directory.CreateDirectory(GameManager.CurrentWorldPath);
        
        string playerPath = Path.Combine(GameManager.CurrentWorldPath, "PlayerTransform.json");
        File.WriteAllBytes(playerPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(ctx, true)));
    }

    public override bool Load(FileIdentifier fileIdentifier, out OutputContext outputContext)
    {
        string playerPath = Path.Combine(GameManager.CurrentWorldPath, "PlayerTransform.json");
        if (File.Exists(playerPath))
        {
            string json = "";
            try
            {
                json = File.ReadAllText(playerPath);
            }
            catch (Exception)
            {
                Debug.Log($"Could not load player transform from {playerPath}");
            }

            if (json != "")
            {
                PlayerIOContext context = JsonUtility.FromJson<PlayerIOContext>(json);
                outputContext = context;

                return true;
            }
            else
            {
                Debug.Log("Formatting went wrong");
            }
        }
        
        outputContext = null;
        return false;
    }
}


public struct PlayerFileIdentifier : FileIdentifier
{
    
}

[Serializable]
public class PlayerIOContext : SavingContext, OutputContext
{
    public Vector3 PlayerPosition;
    public Quaternion playerRotation;
    public Quaternion cameraRotation;
    public bool UseGravity = true;

    public PlayerIOContext(Vector3 pos, Quaternion playerRotation, Quaternion cameraRotation, bool useGravity)
    {
        this.PlayerPosition = pos;
        this.playerRotation = playerRotation;
        this.cameraRotation = cameraRotation;
        this.UseGravity = useGravity;
    }
}
