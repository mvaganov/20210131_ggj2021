using MazeGeneration;
using NonStandard;
using System.Collections.Generic;
using UnityEngine;



public class MazeLevel : MonoBehaviour
{
    public TextAsset mazeSrc;
    public MazeTile prefab_mazeTile;
    Map2d map;
    public float wallHeight = 4;
    List<MazeTile> mazeTiles = new List<MazeTile>();
    public Discovery discoverer;
    [System.Serializable] public class MazeGenArgs {
        public Vector2 size = new Vector2(21, 21), start = Vector2.one, step = Vector2.one, wall = Vector2.one;
        public int seed = -1, erosion = 0;
	}
    public MazeGenArgs mArgs = new MazeGenArgs();
    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public void Generate() {
        if (mazeSrc == null) {
            int seed = mArgs.seed;
            if (seed < 0) {
                seed = (int)Clock.NowRealTicks;
            }
            MazeGenerator mg = new MazeGenerator(seed);
            char[,] map = mg.Generate(mArgs.size, mArgs.start, mArgs.step, mArgs.wall);
            mg.Erode(map, mArgs.erosion);
            this.map = new Map2d(map);
        } else {
            map.LoadFromString(mazeSrc.text);
        }
        int count = map.Height * map.Width;
        while(mazeTiles.Count < count) {
            mazeTiles.Add(Instantiate(prefab_mazeTile.gameObject).GetComponent<MazeTile>());
		}
        int index = 0;
        Vector3 off = new Vector3(map.Width/-2f * discoverer.tileSize.x, 0, map.Height/-2f * discoverer.tileSize.z);
        Transform selfT = transform;
        map.GetSize().ForEach(c => {
            MazeTile mt = mazeTiles[index++];
            mt.coord = c;
            Transform t = mt.transform;
            t.SetParent(selfT);
            t.localPosition = mt.CalcLocalPosition(discoverer);
            mt.kind = map[c] == '#' ? MazeTile.Kind.Wall : MazeTile.Kind.Floor;
            mt.SetDiscovered(false, discoverer);
        });
    }

}
