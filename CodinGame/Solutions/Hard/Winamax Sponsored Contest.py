import sys
sys.setrecursionlimit(10**7)

w, h = map(int, sys.stdin.readline().split())
grid = [list(sys.stdin.readline().rstrip('\n')) for _ in range(h)]
balls = []
holes = set()
for y in range(h):
    for x in range(w):
        c = grid[y][x]
        if c == 'H':
            holes.add((x, y))
        elif c.isdigit() and c != '0':
            balls.append((x, y, int(c)))

DIRS = {'L': (-1, 0), 'R': (1, 0), 'U': (0, -1), 'D': (0, 1)}
AR = {'L': '<', 'R': '>', 'U': '^', 'D': 'v'}
blocked_all = {(x, y) for x, y, _ in balls} | holes

def gen_paths(ball):
    bx, by, shots = ball
    blocked = blocked_all - {(bx, by)}
    res = []
    def dfs(x, y, s, used, segs):
        if s == 0:
            return
        for d, (dx, dy) in DIRS.items():
            cells = []
            for i in range(s):
                nx, ny = x + dx * i, y + dy * i
                if not (0 <= nx < w and 0 <= ny < h) or (nx, ny) in blocked:
                    cells = None
                    break
                cells.append((nx, ny))
            if cells is None:
                continue
            lx, ly = x + dx * s, y + dy * s
            if not (0 <= lx < w and 0 <= ly < h) or grid[ly][lx] == 'X':
                continue
            if (lx, ly) in blocked and grid[ly][lx] != 'H':
                continue
            if any(c in used for c in cells):
                continue
            nu = used | set(cells)
            ns = segs + [(d, cells)]
            if (lx, ly) in holes:
                res.append((nu, (lx, ly), ns))
            else:
                dfs(lx, ly, s - 1, nu, ns)
    dfs(bx, by, shots, set(), [])
    return res

paths = [gen_paths(b) for b in balls]
order = sorted(range(len(balls)), key=lambda i: len(paths[i]))
assign = [None] * len(balls)
used_holes = set()
used_cells = set()

def backtrack(idx):
    if idx == len(balls):
        return True
    i = order[idx]
    for used, hole, segs in paths[i]:
        if hole in used_holes or used & used_cells:
            continue
        assign[i] = segs
        used_holes.add(hole)
        prev = set(used_cells)
        used_cells.update(used)
        if backtrack(idx + 1):
            return True
        used_holes.remove(hole)
        used_cells.clear()
        used_cells.update(prev)
    assign[i] = None
    return False

if not backtrack(0):
    print("No solution", file=sys.stderr)
    sys.exit(1)

out = [['.' for _ in range(w)] for _ in range(h)]
for segs in assign:
    for d, cells in segs:
        for x, y in cells:
            out[y][x] = AR[d]

print('\n'.join(''.join(row) for row in out))
