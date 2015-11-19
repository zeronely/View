using System;

public class AssetCfg
{
    public readonly string key;
    public readonly string hash;
    public readonly uint size;
    public readonly bool preload;
    public readonly bool needUpdate;

    public AssetCfg(string[] array)
    {
        key = array[0];
        hash = array[1];
        size = Convert.ToUInt32(array[2]);
        preload = string.Equals("1", array[3]);

        needUpdate = false;
    }

    public AssetCfg(string[] array, string localHash)
    {
        key = array[0];
        hash = array[1];
        size = Convert.ToUInt32(array[2]);
        preload = string.Equals("1", array[3]);
        needUpdate = !string.Equals(localHash, hash);
    }

    public override bool Equals(object obj)
    {
        AssetCfg cfg = obj as AssetCfg;
        if (cfg == null) return false;
        return string.Equals(key, cfg.key) && string.Equals(hash, cfg.hash) && size == cfg.size && preload == cfg.preload;
    }

    public override int GetHashCode()
    {
        int _hash = base.GetHashCode();
        if (key != null) _hash += key.GetHashCode();
        if (hash != null) _hash += hash.GetHashCode();
        _hash += size.GetHashCode();
        _hash += preload.GetHashCode();
        return _hash;
    }

    public override string ToString()
    {
        return string.Concat(key, "|", hash, "|", size, "|", preload);
    }
}
