local BattleUI = {}
BattleUI.__index = BattleUI

local Notifier = require 'Framework.Notifier'

function BattleUI.Create()
    local copy = {}
    setmetatable(copy, BattleUI)
    copy:Init()
    return copy
end

function BattleUI:Init()
    self.rootGO = GameObject.Instantiate(Resources.Load("UI/UI_Battle"), GameObject.Find("UI/Canvas2D").transform)
    self.txt_round = self.rootGO.transform:Find("txt_round"):GetComponent("Text")
    self.obj_bottom = self.rootGO.transform:Find("panel_bottom").gameObject
    self.list_skill = {}
    for i=1,4 do
        local item_skill = {}
        item_skill.obj_item = self.obj_bottom.transform:Find("list_skill/item_skill"..i).gameObject
        item_skill.txt_name = item_skill.obj_item.transform:Find("txt_name"):GetComponent("Text")
        item_skill.obj_selected = item_skill.obj_item.transform:Find("img_selected").gameObject
        item_skill.btn_choose = item_skill.obj_item.transform:Find("btn_choose"):GetComponent("Button")
        item_skill.btn_choose.onClick:AddListener(function() Notifier.Dispatch("_Battle_UI_Click_Skill", i) end)
        item_skill.img_cd = item_skill.obj_item.transform:Find("img_cd"):GetComponent("Image")
        table.insert(self.list_skill, item_skill)
    end
    self.btn_skip = self.obj_bottom.transform:Find("btn_skip"):GetComponent("Button")
    self.btn_skip.onClick:AddListener(function() Notifier.Dispatch("_Battle_UI_Click_Skip") end)
    self.rootGO:SetActive(false)

    Notifier.AddListener("_Battle_Start", self.Show, self)
    Notifier.AddListener("_Battle_End", self.Hide, self)
    Notifier.AddListener("_Battle_New_Round", self.AnimRound, self)
    Notifier.AddListener("_Battle_Actor_Input", self.ShowActorSkills, self)
    Notifier.AddListener("_Battle_Skill_Selected", self.ShowChosenSkillAffects, self)
    Notifier.AddListener("_Battle_Actor_Input_Done", self.HideActorSkills, self)
end

function BattleUI:Dispose()
    Notifier.RemoveListener("_Battle_Start", self.Show, self)
    Notifier.RemoveListener("_Battle_End", self.Hide, self)
    Notifier.RemoveListener("_Battle_New_Round", self.AnimRound, self)
    Notifier.RemoveListener("_Battle_Actor_Input", self.ShowActorSkills, self)
    Notifier.RemoveListener("_Battle_Skill_Selected", self.ShowChosenSkillAffects, self)
    Notifier.RemoveListener("_Battle_Actor_Input_Done", self.HideActorSkills, self)

    if self.rootGO == nil then return end
    GameObject.Destroy(self.rootGO)
    self.rootGO = nil
    self.txt_round = nil
    self.obj_bottom = nil
    self.list_skill = nil
end

function BattleUI:Show()
    self.rootGO:SetActive(true)
    self.txt_round.gameObject:SetActive(false)
    self.obj_bottom:SetActive(false)
end

function BattleUI:Hide()
    self.rootGO:SetActive(false)
end

-- 播放回合变更动画
function BattleUI:AnimRound(round)
    self.txt_round.gameObject:SetActive(true)
    self.txt_round.text = "Round "..round
    self.txt_round.transform:DOScale(Vector3(1.5, 1.5, 1), 0.25):SetLoops(2, CS.DG.Tweening.LoopType.Yoyo)
end

--
function BattleUI:ShowActorSkills(actor)
    self.obj_bottom:SetActive(true)
    for i=1,4 do
        local skill = actor.skills[i]
        local item_skill = self.list_skill[i]
        item_skill.obj_item:SetActive(skill ~= nil)
        if skill ~= nil then
            item_skill.img_cd.fillAmount = skill.cfgTbl.intervalCD == 0 and 0 or (skill.cd / skill.cfgTbl.intervalCD)
            item_skill.txt_name.text = skill.cfgTbl.name
            item_skill.obj_selected:SetActive(false)
        end
    end
end

function BattleUI:HideActorSkills()
    self.obj_bottom:SetActive(false)
end

function BattleUI:ShowChosenSkillAffects(skillIdx)
    for i=1,4 do
        local item_skill = self.list_skill[i]
        item_skill.obj_selected:SetActive(i == skillIdx)
    end
end

return BattleUI