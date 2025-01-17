local Skills = {}

Skills[10101] = { -- 主角光环
    id=10101,
    name="免死领域",
    rangeMin=0,
    rangeMax=9,
    startCD=0,
    intervalCD=8,
    priority=0,
    power=0,
    isAOE=true,
    affectRange=1,
    campFilter=1,  -- 1对己方, 2对敌方
    enterAction=nil,  -- 开场action
    castAction="cast10101",  -- 施放action
}
Skills[10201] = { -- 弓手普攻
    id=10201,
    name="射击",
    rangeMin=3,
    rangeMax=9,
    startCD=0,
    intervalCD=0,
    priority=0,
    power=40,
    isAOE=false,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast10201",
}
Skills[10202] = { -- 弓手箭雨
    id=10202,
    name="箭雨",
    rangeMin=5,
    rangeMax=9,
    startCD=0,
    intervalCD=3,
    priority=1,
    power=60,
    isAOE=true,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast10202",
}
Skills[10301] = { -- 法师普攻
    id=10301,
    name="火球术",
    rangeMin=1,
    rangeMax=6,
    startCD=0,
    intervalCD=0,
    priority=0,
    power=50,
    isAOE=false,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast10301",
}
Skills[10302] = { -- 法师火球
    id=10302,
    name="陨石术",
    rangeMin=1,
    rangeMax=6,
    startCD=0,
    intervalCD=3,
    priority=1,
    power=50,
    isAOE=false,
    affectRange=2,
    campFilter=2,
    enterAction=nil,
    castAction="cast10302",
}
Skills[10401] = { -- 牧师普攻
    id=10401,
    name="惩戒",
    rangeMin=1,
    rangeMax=4,
    startCD=0,
    intervalCD=0,
    priority=0,
    power=30,
    isAOE=false,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast10401",
}
Skills[10402] = { -- 牧师治疗
    id=10402,
    name="圣疗",
    rangeMin=0,
    rangeMax=4,
    startCD=0,
    intervalCD=1,
    priority=1,
    power=100,
    isAOE=false,
    affectRange=1,
    campFilter=1,
    enterAction=nil,
    castAction="cast10402",
}
Skills[10501] = { -- 战士普攻
    id=10501,
    name="劈砍",
    rangeMin=1,
    rangeMax=2,
    startCD=0,
    intervalCD=0,
    priority=0,
    power=40,
    isAOE=false,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast10501",
}
Skills[10502] = { -- 战士嘲讽
    id=10502,
    name="嘲讽",
    rangeMin=0,
    rangeMax=0,
    startCD=0,
    intervalCD=2,
    priority=1,
    power=0,
    isAOE=false,
    affectRange=1,
    campFilter=1,
    enterAction=nil,
    castAction="cast10502",
}
Skills[20101] = { -- Boss普攻
    id=20101,
    name="Boss普攻",
    rangeMin=1,
    rangeMax=9,
    startCD=0,
    intervalCD=0,
    priority=0,
    power=50,
    isAOE=false,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast20101",
}
Skills[20102] = { -- Boss大招
    id=20102,
    name="Boss大招",
    rangeMin=1,
    rangeMax=9,
    startCD=3,
    intervalCD=3,
    priority=1,
    power=50,
    isAOE=true,
    affectRange=1,
    campFilter=2,
    enterAction="enter20102",
    castAction="cast20102",
}
Skills[20201] = { -- 小怪普攻
    id=20201,
    name="小怪普攻",
    rangeMin=1,
    rangeMax=9,
    startCD=0,
    intervalCD=0,
    priority=0,
    power=25,
    isAOE=false,
    affectRange=1,
    campFilter=2,
    enterAction=nil,
    castAction="cast20201",
}

return Skills