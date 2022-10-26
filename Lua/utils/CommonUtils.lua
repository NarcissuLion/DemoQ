local CommonUtils = {}

function CommonUtils.CompareTwoLists(listA, listB)
    if #listA ~= #listB then return false end
    for i=1,#listA do
        if listA[i] ~= listB[i] then return false end
    end
    return true
end

function CommonUtils.SafeMultiply(a, b, protection)
    return math.floor(a * b + (protection ~= nil and protection or 0.000001))
end

return CommonUtils