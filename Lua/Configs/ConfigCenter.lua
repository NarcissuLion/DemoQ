ConfigCenter = {}

local spaces = {}
spaces["skill"] = require 'Configs.Skills'
spaces["actor"] = require 'Configs.Actors'

function ConfigCenter.Find(tblName, tblID)
    local space = spaces[tblName]
    assert(space ~= nil, "Error! Space not exists: "..tblName)
    assert(space[tblID] ~= nil, "Error! Config not exists: "..tblName.."->"..tblID)
    return space[tblID]
end

return ConfigCenter