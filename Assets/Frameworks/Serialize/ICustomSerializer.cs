
public interface ICustomSerializer
{
    string Serialize(string emptyStr = "");
    void Deserialize(string str);
}
