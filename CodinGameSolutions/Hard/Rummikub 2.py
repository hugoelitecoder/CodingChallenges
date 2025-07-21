import sys
class Table():
    def __init__(self):
        self.rows = {}
        self.type = ['']
        self.character = ['']
        self.lens = [0]
        self.mins = [0]
        self.maxs = [0]
    
    def __repr__(self):
        return str(self.rows).replace('],','],\n')
    
    def __getitem__(self, item):
        return self.rows[item]

    def copy(self):
        tc = Table()
        for rowid in self.rows:
            tc.addRow(self.rows[rowid])
        return tc
    
    def unique(self):
        tout = ()
        for rowid in self.rows:
            tout += (53,)
            tout += tuple(self.rows[rowid])
        return tout
    
    def addRow(self,row):
        nextid = max(self.rows,default=0) + 1
        self.type.append('')
        self.character.append('')
        self.lens.append(0)
        self.mins.append(0)
        self.maxs.append(0)

        self.rows[nextid] = row.copy()
        self.getInfo(nextid)
        return nextid

    def getInfo(self,rowid):
        rowvalues = self.rows[rowid]
        self.type[rowid] = ''
        self.character[rowid] = ''
        self.lens[rowid] = len(rowvalues)
        if len(rowvalues)<3 or self.lens[rowid] != len(set(rowvalues)):
            return
        J = False
        if 'J' in rowvalues: 
            vals = [val for val in rowvalues if val!='J']
            J = True
        else: 
            vals = rowvalues.copy()
        numbers = [int(val[:-1]) for val in vals]
        colors = [val[-1] for val in vals]
        
        self.mins[rowid] = min(numbers)
        self.maxs[rowid] = max(numbers)

        leng = self.lens[rowid]
        minnum = self.mins[rowid]
        maxnum = self.maxs[rowid]
        if minnum == maxnum:
            self.type[rowid] = 'S'
            self.character[rowid] = str(minnum)
            self.sort(rowid,rowvalues,vals,'S',J)
        elif (len(set(colors))==1) and ((maxnum - minnum + 1 == leng) or (J and (maxnum - minnum + 2 == leng))):
            self.type[rowid] = 'R'
            self.character[rowid] = colors[0]
            self.sort(rowid,rowvalues,vals,'R',J)
 
    def sort(self,rowid,rowvalues,valsxJ,rowtype,Jinrow):
        if Jinrow and rowtype=='R':
            valsxJ.sort(key=lambda x: int(x[:-1]))
            if self.maxs[rowid] - self.mins[rowid] + 1 == len(valsxJ):
                valsxJ.append('J')
            else:
                allvals = range(self.mins[rowid],self.maxs[rowid]+1)
                allnums = [int(val[:-1]) for val in valsxJ]
                missing = [i for i,val in enumerate(allvals) if val not in allnums][0]
                valsxJ.insert(missing,'J')
            self.rows[rowid] = valsxJ
        elif Jinrow and rowtype == 'S':
            valsxJ.sort(key=lambda x: x[-1])
            self.rows[rowid] = valsxJ + ['J']
        else:
            rowvalues.sort(key = lambda x: int(x[:-1]))
            rowvalues.sort(key = lambda x: x[-1])
            self.rows[rowid] = rowvalues
 
    def getTiles(self):
        tiles = []
        for row in self.rows.values():
            tiles += row
        return tiles

    def jokerValues(self):
        jvx = {}
        for rowid in self.rows:
            rowvalues = self.rows[rowid]            
            if 'J' in rowvalues:
                if self.type[rowid] == 'R':
                    color = self.character[rowid]
                    ji = rowvalues.index('J')
                    if rowvalues.index('J') == 0 or rowvalues[::-1].index('J') == 0:
                        jvx[rowid] = [f'{self.mins[rowid]-1}{color}',f'{self.maxs[rowid]+1}{color}']
                    else:
                        number = int((int(rowvalues[ji-1][:-1])+int(rowvalues[ji+1][:-1]))/2)
                        jvx[rowid] = [f'{number}{color}']
                elif self.type[rowid] == 'S':
                    number = self.character[rowid]
                    missing = [f'{number}{color}' for color in 'BGRY' if f'{number}{color}' not in rowvalues]
                    jvx[rowid] = missing
        return jvx
    
    def allJokers(self):
        allJ = []
        jvx = self.jokerValues()
        for jx in jvx.values():
            allJ += jx
        return allJ
    
    def tryCOMBINE(self):
        combinations = []
        for color in 'BRGY':
            rowids = [x for x in self.rows if self.character[x] == color]
            if len(rowids)<2:
                continue
            for i in range(len(rowids)-1):
                for j in range(i+1,len(rowids)):
                    rowid1 = rowids[i]
                    row1 = self.rows[rowid1]
                    rowid2 = rowids[j]
                    row2 = self.rows[rowid2]
                    if self.mins[rowid1] == self.maxs[rowid2]+1 or self.mins[rowid2] == self.maxs[rowid1]+1:
                        combinations.append([rowid1,rowid2])
                    elif 'J' in row1 or 'J' in row2:
                        jvx = self.jokerValues()
                        if rowid1 in jvx:
                            if len(jvx[rowid1]) == 2 and (self.mins[rowid1] == self.maxs[rowid2]+2 or self.mins[rowid2] == self.maxs[rowid1]+2):
                                combinations.append([rowid1,rowid2])
                        elif rowid2 in jvx:
                            if len(jvx[rowid2]) == 2 and (self.mins[rowid1] == self.maxs[rowid2]+2 or self.mins[rowid2] == self.maxs[rowid1]+2):
                                combinations.append([rowid1,rowid2])
                            
        return combinations
        
    def tryPUT(self,tile_str):
        possibles = []
        color = tile_str[-1]
        number = int(tile_str[:-1])
        # Try sets
        rowids = [x for x in self.rows if self.character[x] == str(number)]
        for rowid in rowids:
            if tile_str not in self.rows[rowid]:
                possibles.append(rowid)
        # Try runs
        rowids = [x for x in self.rows if self.character[x] == color]
        jvx = self.jokerValues()
        for rowid in rowids:
            run = self.rows[rowid]
            if 'J' in run and tile_str in jvx[rowid]:
                possibles.append(rowid)
            elif number == self.maxs[rowid]+1 or number == self.mins[rowid]-1:
                possibles.append(rowid)
            elif number == self.maxs[rowid]+2 or number == self.mins[rowid]-2:
                if run[0] == 'J' or run[-1] == 'J':
                    possibles.append(rowid)
            else: 
                runvalues = run.copy()
                if 'J' in runvalues:
                    if run[0] == 'J' or run[-1] == 'J':
                        runvalues.insert(0,jvx[rowid][0])
                    runvalues[runvalues.index('J')] = jvx[rowid][-1]
                if tile_str in runvalues:
                    if runvalues.index(tile_str) >= 2 and runvalues[::-1].index(tile_str) >= 2:
                        possibles.append(rowid)
        return possibles
        
    def tryTAKE(self,tile_str):
        possibles = []
        color = tile_str[-1]
        number = int(tile_str[:-1])
        # Try sets
        rowids = [x for x in self.rows if self.character[x] == str(number)]
        for rowid in rowids:
            if self.lens[rowid]==4:
                possibles.append(rowid)
        # Try runs
        rowids = [x for x in self.rows if self.character[x] == color]
        jvx = self.jokerValues()
        for rowid in rowids:
            run = self.rows[rowid]
            if (number == self.maxs[rowid] or number == self.mins[rowid]) and self.lens[rowid]>=4:
                possibles.append(rowid)
            elif (number == self.maxs[rowid]+1 or number == self.mins[rowid]-1) and self.lens[rowid]>=4:
                if rowid in jvx and len(jvx[rowid]) == 2:
                    possibles.append(rowid)
            elif self.lens[rowid]>=7:
                runvalues = run.copy()
                if 'J'in runvalues:
                    runvalues[runvalues.index('J')] = jvx[rowid][0]
                if number in range(self.mins[rowid],self.maxs[rowid]+1):
                    if runvalues.index(tile_str) >= 3 and runvalues[::-1].index(tile_str) >= 3:
                        possibles.append(rowid)
        return possibles        

    def COMBINE(self,rowidA,rowidB):
        self.rows[rowidA] = self.rows[rowidA] + self.rows[rowidB]
        self.rows[rowidB] = []
        self.getInfo(rowidA)
        self.getInfo(rowidB)
    
    def PUT(self,tile_str,rowid):
        if tile_str not in self.rows[rowid]:
            self.rows[rowid] = self.rows[rowid] + [tile_str]
            self.getInfo(rowid)
        else:
            rowvalues = self.rows[rowid]
            tilei = rowvalues.index(tile_str)
            if rowvalues[-1] == 'J' and tilei==1:
                row1 = rowvalues[:tilei+1] + ['J']
                row2 = rowvalues[tilei:-1]
            else:
                row1 = rowvalues[:tilei+1]
                row2 = rowvalues[tilei:]
            self.rows[rowid] = row1
            self.getInfo(rowid)
            self.addRow(row2)        

    def TAKE(self,tile_str,rowid):
        rowvalues = self.rows[rowid].copy()
        J = False
        if 'J'in rowvalues:
            J = True
            jix = self.jokerValues()[rowid]
            rowvalues[rowvalues.index('J')] = jix[-1]
            if len(jix) == 2:
                rowvalues.insert(0,jix[0])

        if tile_str == rowvalues[0] or tile_str == rowvalues[-1] or self.type[rowid]=='S':
            if J and tile_str in jix:
                self.rows[rowid].remove('J')
                self.getInfo(rowid)
            else:
                self.rows[rowid].remove(tile_str)
                self.getInfo(rowid)
        elif J and len(jix)==2 and (tile_str == rowvalues[1] or tile_str == rowvalues[-2]):
            self.rows[rowid].remove(tile_str)
            self.getInfo(rowid)
        else:
            tilei = rowvalues.index(tile_str)
            row1 = rowvalues[:tilei]
            row2 = rowvalues[tilei+1:]
            self.rows[rowid] = row1
            self.getInfo(rowid)
            self.addRow(row2)


def findMissingPut(tile_str,table):
    # Missing tiles to put away
    missing_options = {}
    color = tile_str[-1]
    number = int(tile_str[:-1])
    
    # 'Missing' to put in a set
    rowids = [x for x in table.rows if table.type[x] == 'S' and table.character[x] == str(number)]
    for rowid in rowids:
        if tile_str not in table.rows[rowid]:
            missing_options[rowid] = []
    
    # Missing to put in a row
    rowids = [x for x in table.rows if table.type[x] == 'R' and table.character[x] == color]
    jvx = table.jokerValues()
    for rowid in rowids:
        run = table.rows[rowid]
        if 'J' in run and tile_str in jvx[rowid]:
            missing_options[rowid] = []
        elif number > table.maxs[rowid]:
            missing = []
            for nx in range(table.maxs[rowid]+1,number):
                missing.append(f'{nx}{color}')
            missing_options[rowid] = missing
        elif number < table.mins[rowid]:
            missing = []
            for nx in range(number+1,table.mins[rowid]):
                missing.append(f'{nx}{color}')
            missing_options[rowid] = missing
        else:
            if number == table.mins[rowid] and number>=3:
                missing_options[rowid] = [f'{table.mins[rowid]-2}{color}',f'{table.mins[rowid]-1}{color}']
            elif number == table.maxs[rowid] and number <= 11:
                missing_options[rowid] = [f'{table.maxs[rowid]+1}{color}',f'{table.maxs[rowid]+2}{color}']
            elif number == table.mins[rowid]+1 == table.maxs[rowid]-1:
                missing_options[rowid] = [f'{table.mins[rowid]-1}{color}',f'{table.maxs[rowid]+1}{color}']
            elif number == table.mins[rowid]+1:
                missing_options[rowid] = [f'{table.mins[rowid]-1}{color}']
            elif number == table.maxs[rowid]-1:
                missing_options[rowid] = [f'{table.maxs[rowid]+1}{color}']
            elif run[::-1].index(tile_str) >= 2 and run.index(tile_str) >= 2:
                missing_options[rowid] = []
    return missing_options

def findMissingTake(tile_str,table):
    # Missing tiles to put away
    missing_options = {}
    color = tile_str[-1]
    number = int(tile_str[:-1])
    
    # Missing to take from a set
    jvx = table.jokerValues()
    rowids = [x for x in table.rows if table.type[x] == 'S' and (table.character[x] == str(number) or x in jvx)]
    for rowid in rowids:
        setvalues = table.rows[rowid]
        if tile_str not in setvalues and 'J' not in setvalues:
            continue
        if table.lens[rowid]==4:
            missing_options[rowid] = []
        else:
            missing_options[rowid] = [f'{number}{c}' for c in 'BGRY' if f'{number}{c}' not in setvalues]
        
    # Missing to take from a run
    rowids = [x for x in table.rows if table.type[x] == 'R' and table.character[x] == color]
    for rowid in rowids:
        run = table.rows[rowid].copy()
        runvalues = run.copy()
        if tile_str not in runvalues:
            continue
        else:
            if number == table.mins[rowid]:
                if table.lens[rowid] >= 4:
                    missing_options[rowid] = []
                elif number < 11:
                    missing_options[rowid] = [f'{table.maxs[rowid]+1}{color}']
            elif number == table.maxs[rowid]:
                if table.lens[rowid] >= 4:
                    missing_options[rowid] = []
                elif number>3:
                    missing_options[rowid] = [f'{table.mins[rowid]-1}{color}']   
            elif number <= 3 or number >= 11:
                continue #Taking a 3 or 11 not at the end is never possible by finding missing tiles as helpers
            
            elif number == table.mins[rowid]+1:
                missing = [f'{table.mins[rowid]-2}{color}',f'{table.mins[rowid]-1}{color}']
                if number == table.maxs[rowid]-1:
                    missing += [f'{table.maxs[rowid]+1}{color}',f'{table.maxs[rowid]+2}{color}']
                elif number == table.maxs[rowid]-2:
                    missing.append(f'{table.maxs[rowid]+1}{color}')
            elif number == table.mins[rowid]+2:
                missing = [f'{table.mins[rowid]-1}{color}']
                if number == table.maxs[rowid]-1:
                    missing += [f'{table.maxs[rowid]+1}{color}',f'{table.maxs[rowid]+2}{color}']
                elif number == table.maxs[rowid]-2:
                    missing.append(f'{table.maxs[rowid]+1}{color}')
            else:
                missing = []
                if number == table.maxs[rowid]-1:
                    missing += [f'{table.maxs[rowid]+1}{color}',f'{table.maxs[rowid]+2}{color}']
                elif number == table.maxs[rowid]-2:
                    missing.append(f'{table.maxs[rowid]+1}{color}')
            try:
                missing_options[rowid] = missing
            except:
                continue

    return missing_options

def findNuisanceTake(tile_str,table):
    # Possible to take if some tiles are TAKE and PUT somewhere else first
    missing_options = {}
    color = tile_str[-1]
    number = int(tile_str[:-1])
           
    # Only the ones where removal never leads to an invalid run
    rowids = [x for x in table.rows if table.type[x] == 'R' and table.character[x] == color]
    for rowid in rowids:
        if tile_str not in table.rows[rowid]:
            continue
        else:
            # Check for low-nuisance in combination with high-missing
            nuisance = ['x']
            missing = ['x']              
            if number == table.mins[rowid]+1:
                nuisance = [f'{table.mins[rowid]}{color}']
                missing = []
                if number >= 11:
                    nuisance = ['x']
                    missing = ['x']                
                elif number == table.maxs[rowid]-1:
                    missing = [f'{table.maxs[rowid]+1}{color}',f'{table.maxs[rowid]+2}{color}']
                elif number == table.maxs[rowid]-2:
                    missing = [f'{table.maxs[rowid]+1}{color}']
            elif number == table.mins[rowid]+2:
                nuisance = [f'{table.mins[rowid]}{color}',f'{table.mins[rowid]+1}{color}']
                missing = []
                if number >= 11:
                    nuisance = ['x']
                    missing = ['x']                    
                elif number == table.maxs[rowid]-1:
                    missing += [f'{table.maxs[rowid]+1}{color}',f'{table.maxs[rowid]+2}{color}']
                elif number == table.maxs[rowid]-2:
                    missing.append(f'{table.maxs[rowid]+1}{color}')
            if nuisance[0] != 'x':
                newid = max(missing_options,default=-1)+1
                missing_options[newid] = {rowid:[nuisance,missing]}
            
            # Check for high_nuisance in combination with low-missing:
            nuisance = ['x']
            missing = ['x']              
            if number == table.maxs[rowid]-1:
                nuisance = [f'{table.maxs[rowid]}{color}']
                missing = []
                if number <= 3:
                    nuisance = ['x']
                    missing = ['x']                
                elif number == table.mins[rowid]+1:
                    missing = [f'{table.mins[rowid]-2}{color}',f'{table.mins[rowid]-1}{color}']
                elif number == table.mins[rowid]+2:
                    missing = [f'{table.mins[rowid]-1}{color}']
            elif number == table.maxs[rowid]-2:
                nuisance = [f'{table.maxs[rowid]}{color}',f'{table.maxs[rowid]-1}{color}']
                missing = []
                if number <= 3:
                    nuisance = ['x']
                    missing = ['x']                    
                elif number == table.mins[rowid]+1:
                    missing += [f'{table.mins[rowid]-2}{color}',f'{table.mins[rowid]-1}{color}']
                elif number == table.mins[rowid]+2:
                    missing.append(f'{table.mins[rowid]-1}{color}')
            if nuisance[0] != 'x':
                newid = max(missing_options,default=-1)+1
                missing_options[newid] = {rowid:[nuisance,missing]}          

    return missing_options     
    
class Taskroads():
    def __init__(self,goalTile,table,stonetasks):
        self.used_combines = []
        self.used_splits = {}
        self.unique_tables = {}
        self.all_roads = {'':table}
        self.no_solution = True
        self.goalTile = goalTile
        self.table = table.copy()
        self.solution_len = float('inf')
        self.solutions = {}
        self.archive = {}
        self.stonetasks = stonetasks
    
    def solved(self):
        return not self.no_solution

    def executeTaskroad(self,road,table,check_splits = False):
        for task in road.split(';'):
            if task:
                if len(tasksplit:=task.split()) == 3:
                    command, tile_str, rowid = tasksplit
                else:
                    command, tile_str, rowid, _ = tasksplit
                if command=='P':
                    table.PUT(tile_str,int(rowid))
                elif command == 'T':
                    lastid = max(table.rows)
                    table.TAKE(tile_str,int(rowid))
                    if max(table.rows)>lastid and check_splits:
                        ri1 = rowid
                        ri2 = max(table.rows)
                        if ri1 in self.used_splits:
                            self.used_splits[ri1].add(ri2)
                        else:
                            self.used_splits[ri1] = set([ri2])
                        if ri2 in self.used_splits:
                            self.used_splits[ri2].add(ri1)
                        else:
                            self.used_splits[ri2] = set([ri1])
                elif command == 'C':
                    table.COMBINE(int(tile_str),int(rowid))

    def nextStep(self,taskroad,table_c):
        news = []
        if len(taskroad.split(';'))>=self.solution_len: # Never continue a longer road
            return news
        previous_task = taskroad.split(';')[-1]
        if previous_task == '' or previous_task[0] in 'CP':
            # 1: Try goalTile
            goalTile = self.goalTile
            if (puts:=table_c.tryPUT(goalTile)):
                for rowid in puts:
                    if f'T {goalTile} {rowid:02d}' in taskroad:
                        continue
                    if taskroad:
                        new = taskroad + ';' + f'P {goalTile} {rowid:02d}'
                    else: new = f'P {goalTile} {rowid:02d}'
                    self.no_solution = False
                    if len(new.split(';'))<self.solution_len:
                        self.solution_len = len(new.split(';'))                    
                    if new not in news:
                        news.append(new)
            
            # 2: Try COMBINE                
            if len(taskroad.split(';')) >= self.solution_len-1: # Never continue a longer road
                return news
            if (combines:=table_c.tryCOMBINE()):
                for combi in combines:
                    combi.sort()
                    if (combi not in self.used_combines) or ('C' in taskroad and f'C {combi[0]:02d} {combi[1]:02d}' not in taskroad):
                        self.used_combines += [combi]
                        if taskroad:
                            new = taskroad + ';' + f'C {combi[0]:02d} {combi[1]:02d}'
                        else: new = f'C {combi[0]:02d} {combi[1]:02d}'
                        if new not in news:
                            news.append(new)
            
            # 3: Try TAKE
            if len(taskroad.split(';')) >= self.solution_len-2: # Never continue a longer road
                return news
            jvx = table_c.jokerValues()
            tiles = table_c.getTiles()
            if (allJ:=table_c.allJokers()):
                while 'J' in tiles:
                    tiles.remove('J')
                tiles += allJ
            for tile_str in tiles:
                if tile_str not in self.stonetasks and tile_str not in allJ:
                    continue
                if (takes:=table_c.tryTAKE(tile_str)):
                    for rowid in takes:
                        if f'P {tile_str} {rowid:02d}' in taskroad:
                            continue
                        if taskroad:
                            new = taskroad + ';' + f'T {tile_str} {rowid:02d}'
                        else: new = f'T {tile_str} {rowid:02d}'
                        if tile_str in table_c.allJokers() and rowid in jvx:
                            new += ' |J'
                        if new not in news:
                            news.append(new)
    
        elif previous_task[0] == 'T':
            # Try PUT
            if len(taskroad.split(';')) >= self.solution_len-1: # Never continue a longer road
                return news
            tile_str = previous_task.split()[1]
            if previous_task.split()[-1] == '|J':
                for rowid in [ri for ri in table_c.rows if table_c.character[ri] == self.goalTile[-1]]:
                    if f'P J {rowid:02d}' in taskroad or f'{rowid:02d} |J' in taskroad:
                        continue
                    new = taskroad + ';' + f'P J {rowid:02d}'
                    news.append(new)
            elif (puts:=table_c.tryPUT(tile_str)):
                for rowid in puts:
                    if f'T {tile_str} {rowid:02d}' in taskroad: #Never PUT in the same row it is TAKEN from
                        continue
                    if rowid in self.used_splits: #Never PUT in a row that is a partner of a row from which it was TAKEN
                        loop = False
                        for partnerid in self.used_splits[rowid]:
                            if f'T {tile_str} {int(partnerid):02d}' in taskroad:
                                loop = True
                                break
                        if loop:
                            continue
                    new = taskroad + ';' + f'P {tile_str} {rowid:02d}'
                    if rowid in table_c.jokerValues() and tile_str in table_c.allJokers():
                        new += ' |J'
                    if new not in news:
                        news.append(new)
        return news

    def allRoads(self,max_iteration=1000):
        i=0
        while self.all_roads and i<max_iteration:
            to_add = {}
            for road,table_c in self.all_roads.items():
                #if road == 'T 6G 01 |J': break
                news = self.nextStep(road,table_c)
                for new in news:
                    tc = self.table.copy()
                    check_splits = False
                    if new.split(';')[-1][0] == 'T':
                        check_splits = True
                    self.executeTaskroad(new,tc,check_splits)
                    # If it is a solution, add to solutions
                    if new.split(';')[-1].startswith(f'P {goalTile}'):
                        self.solutions[new] = tc
                        continue            
                    unique_table = tc.unique()
                    if unique_table not in self.unique_tables:
                        to_add[new] = tc
                        self.unique_tables[unique_table] = new
                        self.archive[unique_table] = []
                    else:
                        self.archive[unique_table].append(new)
            self.all_roads = to_add
            i+=1
        return self


def subTable(table, puts = [], takes = [], rows = []):
    ## Either start with the goalTile in puts, or with takes from the initial findMissingPuts
    
    tasks = {'puts':puts,'takes':takes}
    checked = {'puts':[],'takes':[]}
    tablevalues = table.getTiles()
    njx = tablevalues.count('J')
    stonetasks = []
    
    ## Add rows with jokers anyway to the subTable
    jvx = table.jokerValues()
    for rowid in jvx:
        rows.append(rowid)
        for jvalue in table.allJokers():
            if (possible_rows := table.tryTAKE(jvalue)):
                simple = False
                for rowid in possible_rows:
                    if rowid in jvx:
                        simple = True
                if not simple:
                    tasks['takes'].append(jvalue)
            else:
                tasks['takes'].append(jvalue)
    stonetasks = stonetasks + tasks['takes'] + table.allJokers()
    
    # Do a first check of possibility anyway on the initial takes
    if len(tasks['takes'])>0:
        potential = True
        for tile_str in tasks['takes']:
            if tile_str not in tablevalues and tile_str not in table.allJokers() and njx==0:
                potential = False
                break
            elif tile_str not in tablevalues:
                njx -= 1
        if not potential:
            rows = []
            tasks['takes'] = []   
    njx = tablevalues.count('J')
    while tasks['puts'] or tasks['takes']:
        stonetasks = stonetasks + tasks['puts'] + tasks['takes']
        # move through puts
        if tasks['puts']:
            to_put = tasks['puts'].pop(0)
            if to_put not in checked['puts']:
                checked['puts'].append(to_put)
                missingputs = findMissingPut(to_put,table)
                for key,value in missingputs.items():
                    if key in rows:
                        continue
                    potential = True
                    for tile_str in value:
                        if tile_str not in tablevalues and njx==0:
                            potential = False
                            break
                        elif tile_str not in tablevalues:
                            njx -= 1
                    if potential: 
                        rows.append(key)
                        for tile_str in value:
                            if tile_str not in tasks['takes']:
                                tasks['takes'] += value
        njx = tablevalues.count('J')
        # move through takes
        if tasks['takes']:
            to_take = tasks['takes'].pop(0)
            if to_take not in checked['takes']:
                checked['takes'].append(to_take)
                missingtakes = findMissingTake(to_take,table)
                for key,value in missingtakes.items():
                    if key in rows:
                        continue
                    potential = True
                    for tile_str in value:
                        if tile_str not in tablevalues and njx==0:
                            potential = False
                            break
                        elif tile_str not in tablevalues:
                            njx -= 1
                    if potential: 
                        rows.append(key)
                        for tile_str in value:
                            if tile_str not in tasks['takes']:
                                tasks['takes'] += value
                njx = tablevalues.count('J')
                missing_nuisance = findNuisanceTake(to_take,table)
                for option in missing_nuisance.values():
                    for row,(nuisance,missing) in option.items():
                        if row in rows:
                            continue
                        potential = True
                        for tile_str in missing:
                            if tile_str not in tablevalues and njx == 0:
                                potential = False
                                break
                            elif tile_str not in tablevalues:
                                njx -= 1
                        for tile_str in nuisance:
                            if not findMissingPut(tile_str,table):
                                potential = False
                                break
                        if potential:
                            rows.append(row)
                            for tile_str in missing:
                                if tile_str not in tasks['takes']:
                                    tasks['takes'] += missing
                            for tile_str in nuisance:
                                if tile_str not in tasks['puts']:
                                    tasks['puts'] += nuisance
        njx = tablevalues.count('J')
    
    # assumption: two idential rows will never both be relevant
    uniquerows = []
        
    newtable = table.copy()
    for rowid in newtable.rows:
        row = newtable.rows[rowid]
        if rowid not in rows or tuple(row) in uniquerows:
            newtable.rows[rowid] = []
            newtable.getInfo(rowid)
        else:
            uniquerows.append(tuple(row))
    return newtable,rows,stonetasks


def select_best_solution(taskdb):
    # Get all solutions from archive
    all_solutions = [solution for solution in taskdb.solutions.keys() if len(solution.split(';'))<=taskdb.solution_len]
    
    for roadpart,equivalents in taskdb.archive.items():
        for newpart in equivalents:
            if len(newpart.split(';'))<=len(roadpart.split(';')):
                found = False
                for solution in all_solutions:
                    if newpart not in solution:
                        if roadpart in solution:
                            found = True
                        new_solution = solution.replace(roadpart,newpart)
                        if new_solution not in all_solutions:
                            all_solutions.append(new_solution)
                    else:
                        found = True
                if not found:
                    print('warning, part to replace not found')
    
    new_solutions = {}
    for solution in all_solutions:
        new_solution_parts = []
        for part in solution.split(';'):
            if 'T' in part and '|J' in part:
                a,b,c,d = part.split()
                part = ' '.join([a,'J',c,d])
            new_solution_parts.append(part)
        new_solution = ';'.join(new_solution_parts)
        new_solutions[new_solution] = solution
    all_solutions = list(set(new_solutions.keys()))

    
    if len(all_solutions) == 1:
        return new_solutions[all_solutions[0]]
    
    # Low C before high C
    classifications = {}
    for solution in all_solutions:
        Csum = [0]
        for i,part in enumerate(solution.split(';')):
            if 'C' in part:
                _,r1,r2 = part.split()
                r1 = int(r1)
                r2 = int(r2)
                Csum.append(Csum[-1]+r1+r2)
        classifications[solution] = sum(Csum)

    all_solutions = [solution for solution in classifications if classifications[solution] == min(classifications.values())]
    
    if len(all_solutions) == 1:
        return new_solutions[all_solutions[0]]
    
    # J preferably not
    noJ = [solution for solution in all_solutions if 'J' not in solution]
    if noJ:
        if len(noJ)>1:
            print('Warning, multiple best solutions. Returning the first')
        return new_solutions[noJ[0]]
        
    # If J, then as early as possible. If PUT J: as early as possible. First, check for put
    jPut = [solution for solution in all_solutions if 'P J' in solution]
    if jPut:
        search_string = 'P J'
        all_solutions = jPut
    else:
        search_string = 'J'
    jpositions = {i:float('inf') for i in range(len(all_solutions))}
    for i in range(len(all_solutions)):
        solution = all_solutions[i]
        solsteps = solution.split(';')
        for j,step in enumerate(solsteps):
            if search_string in step:
                jpositions[i] = j
                break
    nxbest = list(jpositions.values()).count(min(jpositions.values()))
    if nxbest>1:
        print('Warning, multiple best solutions. Returning the first')
    return new_solutions[all_solutions[min(jpositions,key=lambda x: jpositions[x])]]

# Get all possible solutions based on multiple sub_tables
def solve(goalTile,table):
    import time
    start = time.time()
    solution = False
    sub_tables = []
    rowss = []
    stonetaskss = []
    missingputs = findMissingPut(goalTile,table)
    for row,takes in missingputs.items():
        sub_table,rows,stonetasks = subTable(table,takes=takes,rows=[row])
        sub_tables.append(sub_table)
        rowss.append(rows)
        stonetaskss.append(stonetasks)
    trial_order = sorted(range(len(rowss)),key=lambda x: len(rowss[x]))
    trial_order += [len(rowss)]
    rowss += [list(table.rows.keys())]
    sub_tables += [table]
    stonetaskss += [table.getTiles()]
    for i in trial_order:
        sub_table = sub_tables[i]
        taskdb = Taskroads(goalTile,sub_table,stonetaskss[i])
        taskdb = taskdb.allRoads()
        solution = taskdb.solved()
        if solution:
            break
    if solution:
        # Get the solution table
        sub_table = sub_tables[i]
        rows = rowss[i]
        nrows = [rowid for rowid in table.rows if rowid not in rows]

        new_archive = {}
        roads_to_check = [';'.join(solution.split(';')[:-1]) for solution in taskdb.solutions.keys()]
        roads_checked = []
        while roads_to_check:
            road_to_check = roads_to_check.pop(0)
            if road_to_check in roads_checked:
                continue
            roads_checked.append(road_to_check)
            roadsplit = road_to_check.split(';')
            for j in range(2,len(roadsplit)+1):
                subroad = ';'.join(roadsplit[:j])
                tc = sub_table.copy()
                taskdb.executeTaskroad(subroad,tc)
                unique_table = tc.unique()
                equivalents = [road for road in taskdb.archive[unique_table] if len(road.split(';'))<=len(subroad.split(';'))]
                if subroad in new_archive:    
                    new_archive[subroad] += equivalents
                else:
                    new_archive[subroad] = equivalents
                roads_to_check += equivalents
        taskdb.archive = new_archive
        
        # Get the best solution
        best = select_best_solution(taskdb)
        C,P,T = ['COMBINE','PUT','TAKE']
        for task in best.split(';'):
            if len(tasksplit:=task.split()) == 3:
                a1,a2,a3 = tasksplit
            elif tasksplit[0]=='P':
                a1,a2,a3,_ = tasksplit
            else:
                a1,_,a3,_ = tasksplit
                a2 = 'J'
            if a1 == 'C':
                print(eval(a1),int(a2),int(a3))
            else:
                print(eval(a1),a2,int(a3))
        solved_table = sub_table.copy()
        taskdb.executeTaskroad(best,solved_table)
        for rowid in nrows:
            solved_table.rows[rowid] = table.rows[rowid]
            solved_table.getInfo(rowid)
        for rowid in solved_table.rows:
            if solved_table.lens[rowid]>0:
                solved_table.getInfo(rowid)
                print(rowid,*solved_table.rows[rowid])
    else:
        print('Not possible!')
    end = time.time()
    print('Execution time: ',(end-start)* 10**3,'ms',file=sys.stderr,flush=True)
    return solution,taskdb

goalTile = input()
table = Table()
nrow = int(input())
for i in range(nrow):
    table.addRow(input().split()[1:])

solve(goalTile,table)