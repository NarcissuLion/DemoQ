local Actors = {}

Actors[101] = { -- 主角
    id=101,
    maxHP=500,
    speed=1000,
    size=1,
    skills={10101},
    prefab='Player1',
    icon='7',
}
Actors[102] = { -- 弓手
    id=102,
    maxHP=150,
    speed=20,
    size=1,
    skills={10201,10202},
    prefab='Hero3',
    icon='5',
}
Actors[103] = { -- 法师
    id=103,
    maxHP=150,
    speed=12,
    size=1,
    skills={10301,10302},
    prefab='Hero2',
    icon='4',
}
Actors[104] = { -- 牧师
    id=104,
    maxHP=200,
    speed=10,
    size=1,
    skills={10401,10402},
    prefab='Hero4',
    icon='6',
}
Actors[105] = { -- 战士
    id=105,
    maxHP=300,
    speed=18,
    size=1,
    skills={10501,10502},
    prefab='Hero1',
    icon='3',
}
Actors[201] = { -- Boss
    id=201,
    maxHP=1500,
    speed=5,
    size=2,
    skills={20101,20102},
    prefab='Boss1',
    icon='1',
}
Actors[202] = { -- 小怪
    id=202,
    maxHP=100,
    speed=15,
    size=1,
    skills={20201},
    prefab='Monster1',
    icon='2',
}

return Actors