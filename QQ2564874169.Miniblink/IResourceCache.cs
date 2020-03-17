
namespace QQ2564874169.Miniblink
{
    public interface IResourceCache
    {
        bool Matchs(string url);

        byte[] Get(string url);

        void Save(string url, byte[] data);
    }
}
