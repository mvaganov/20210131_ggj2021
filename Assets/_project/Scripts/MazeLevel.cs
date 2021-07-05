using MazeGeneration;
using NonStandard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MazeLevel : MonoBehaviour {
    public TextAsset mazeSrc;
	public MazeTile prefab_mazeTile;
	Map2d map;
    public float undiscoveredHeight = 0.0625f;
    public float floorHeight = 0;
    public float wallHeight = .5f;
    public float animationTime = .25f;
    public Vector3 tileSize = Vector3.one * 4;
    public Vector3 undiscoveredTileSize = Vector3.one;
    public float rampHeight = 1;
    public float rampAngle = 23;
    public float rampScale = 1.5f;
    public Color undiscoveredWall = Color.clear, undiscoveredFloor = Color.clear, undiscoveredRamp = Color.clear;
    public int stage = -1;
    public VisionMapping seen;
    Transform _t;

    List<MazeTile> mazeTiles = new List<MazeTile>();
    public List<MazeTile> floorTiles;
    public int[] floorTileNeighborHistogram = new int[0];
	private void Awake() {
        _t = transform;
        seen = new VisionMapping(() => map != null ? map.GetSize() : new Coord(7,7));
    }
	public Coord GetCoord(Vector3 p) {
        p -= _t.position;
        float x = (p.x) / tileSize.x + 0.5f;
        float y = (p.z) / tileSize.z + 0.5f;
        return new Coord((int)x, (int)y);
    }

    public Vector3 GetPosition(Coord c) { return GetLocalPosition(c)+_t.position; }
    public Vector3 GetLocalPosition(Coord c) { return new Vector3(c.X * tileSize.x, 0, c.Y * tileSize.z); }
    public Vector3 GetGroundPosition(Coord c) {
        MazeTile t = GetTile(c);
        return t?t.CalcVisibilityTarget():GetPosition(c);
    }
    public Map2d Map { get { return map; } }

    [System.Serializable] public class MazeGenArgs {
        public Vector2 size = new Vector2(21, 21), start = Vector2.one, step = Vector2.one, wall = Vector2.one;
        public int seed = -1, erosion = 0;
    }
    public MazeGenArgs mazeGenerationArguments = new MazeGenArgs();
    // Start is called before the first frame update
    public char GetTileSrc(Coord c) { return map[c].letter; }
    public MazeTile GetTile(Coord c) {
        int i = c.Y * map.Width + c.X;
        if (i < 0 || i >= mazeTiles.Count) return null;
        return mazeTiles[i];
    }
    public void Generate(NonStandard.Data.Random random) {
        int width = ((stage + 2) * 2) + 1;
        mazeGenerationArguments.size = new Vector2(width, width);
        if (mazeSrc == null) {
            int seed = mazeGenerationArguments.seed;
            if (seed < 0) {
                seed = (int)GameClock.Time;
            }
            MazeGenerator mg = new MazeGenerator(random.Next);
            char[,] map = mg.Generate(mazeGenerationArguments.size, mazeGenerationArguments.start, mazeGenerationArguments.step, mazeGenerationArguments.wall);
            mg.Erode(map, mazeGenerationArguments.erosion);
            // generate ramps
            Coord size = map.GetSize();
            List<Coord>[] ramp = new List<Coord>[4];
            for(int r = 0; r < ramp.Length; ++r) { ramp[r] = new List<Coord>(); }
            const int W = 0, N = 1, E = 2, S = 3;// todo nwse
            size.ForEach(c => {
                if (c.row == 0 || c.row == size.row - 1 || c.col == 0 || c.col == size.col - 1) return;
                char letter = map.At(c);
                char n = map.At(c + Coord.Up), s = map.At(c + Coord.Down), e = map.At(c + Coord.Right), w = map.At(c + Coord.Left);
                bool createAtDeadEnds = true;
                bool createAtPeninsula = true;
                if (n == s && e != w) {
					if (letter == e && w == n) {
						if (letter == ' ' && createAtDeadEnds) { ramp[W].Add(c); }
						if (letter == '#' && createAtPeninsula) { ramp[E].Add(c); }
					}
					if (letter == w && e == n) {
						if (letter == ' ' && createAtDeadEnds) { ramp[E].Add(c); }
						if (letter == '#' && createAtPeninsula) { ramp[W].Add(c); }
					}
				}
                if(e == w && n != s) {
                    if (letter == n && s == w) {
                        if (letter == ' ' && createAtDeadEnds) { ramp[S].Add(c); }
                        if (letter == '#' && createAtPeninsula) { ramp[N].Add(c); }
                    }
                    if (letter == s && n == e) {
                        if (letter == ' ' && createAtDeadEnds) { ramp[N].Add(c); }
                        if (letter == '#' && createAtPeninsula) { ramp[S].Add(c); }
                    }
                }
            });
            //ramp[W].ForEach(c => { map.SetAt(c, 'w'); });
            //ramp[N].ForEach(c => { map.SetAt(c, 'n'); });
            //ramp[E].ForEach(c => { map.SetAt(c, 'e'); });
            //ramp[S].ForEach(c => { map.SetAt(c, 's'); });
            int totalRamps = ramp.Sum(r=>r.Count);
            for(int i = 0; i < totalRamps && i < stage+1; ++i) {
                int[] r = ramp.GetNestedIndex(random.Next(totalRamps));
                //Debug.Log(r.JoinToString(", "));
                Coord loc = ramp[r[0]][r[1]];
                ramp[r[0]].RemoveAt(r[1]);
                char ch = "wnes"[r[0]];
                map.SetAt(loc, ch);
                --totalRamps;
            }
            this.map = new Map2d(map);
            //Debug.Log(this.map);
        } else {
            map.LoadFromString(mazeSrc.text);
        }
        seen.Reset();
        int count = map.Height * map.Width;
        while (mazeTiles.Count < count) {
            mazeTiles.Add(Instantiate(prefab_mazeTile.gameObject).GetComponent<MazeTile>());
        }
        //int index = 0;
        Vector3 off = new Vector3(map.Width / -2f * tileSize.x, 0, map.Height / -2f * tileSize.z);
        floorTiles = new List<MazeTile>();
        floorTileNeighborHistogram.SetEach(0);
        map.GetSize().ForEach(c => {
            MazeTile mt = GetTile(c);//mazeTiles[index++];
            mt.maze = this;
            mt.coord = c;
            Transform t = mt.transform;
            t.SetParent(_t);
            t.localPosition = mt.CalcLocalPosition();
            MazeTile.Kind k = MazeTile.Kind.None;
			switch (map[c]) {
            case ' ': k = MazeTile.Kind.Floor; break;
            case '#': k = MazeTile.Kind.Wall; break;
            case 'w': k = MazeTile.Kind.RampWest; break;
            case 'n': k = MazeTile.Kind.RampNorth; break;
            case 's': k = MazeTile.Kind.RampSouth; break;
            case 'e': k = MazeTile.Kind.RampEast; break;
            }
            mt.kind = k;
            mt.SetDiscovered(false, null, this);
            if (mt.kind == MazeTile.Kind.Floor) {
                floorTiles.Add(mt);
                mt.goalScore = TileScorer(mt, map);
                int index = (int)mt.goalScore;
                if(index >= floorTileNeighborHistogram.Length) { Array.Resize(ref floorTileNeighborHistogram, index + 1); }
                ++floorTileNeighborHistogram[index];
            } else {
                mt.goalScore = float.PositiveInfinity;
            }
        });
        floorTiles.Sort((a, b) => a.goalScore.CompareTo(b.goalScore));
    }
    public void PlaceObjectOverTile(Transform t, MazeTile mt) {
        Vector3 p = mt.CalcVisibilityTarget() + Vector3.up * tileSize.y;
        t.position = p;
    }
    float TileScorer(MazeTile mt, Map2d map) {
        int neighborFloor = -1;// CountTileNeighbors(mt.coord, _allDirs, c => map[c].letter == '#');//-1;
        Coord msize = map.GetSize();
		Coord min = -Coord.One, max = Coord.One;
		Coord.ForEachInclusive(min, max, off => {
			Coord c = mt.coord + off;
			if (c.IsWithin(msize) && map[c].letter == ' ') { ++neighborFloor; }
		});
		int maxDist = (msize.row + msize.col) / 2;
        float distFromCenter = Mathf.Abs((msize.row - 1) / 2f - mt.coord.row) + Mathf.Abs((msize.col - 1) / 2f - mt.coord.col);
        return neighborFloor + distFromCenter / maxDist;
    }

    int CountTileNeighbors(Coord coord, Coord[] neighborOffsets, Func<Coord,bool> predicate) {
        Coord msize = map.GetSize();
        int count = 0;
        for(int i = 0; i < neighborOffsets.Length; ++i) {
            Coord c = coord + neighborOffsets[i];
            if (c.IsWithin(msize) && predicate.Invoke(c)) { ++count; }
        }
        return count;
	}
    public int CountDiscoveredNeighborWalls(Coord coord) {
        return CountTileNeighbors(coord, Coord.CardinalDirs, c => GetTile(c).discovered && map[c] != ' ');
	}
}
