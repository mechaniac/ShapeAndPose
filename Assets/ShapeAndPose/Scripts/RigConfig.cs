[System.Serializable]
public class Vertex
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class JointConfig
{
    public string name;
    public float scale;
    public Vertex[] vertices;
}

[System.Serializable]
public class LimbConfig
{
    public string name;
    public JointConfig[] joints;
}

[System.Serializable]
public class RigConfig
{
    public LimbConfig[] limbs;
}
