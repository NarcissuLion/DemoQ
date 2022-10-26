local CommonUtils = {}

function CommonUtils.CompareTwoLists(listA, listB)
    if #listA ~= #listB then return false end
    for i=1,#listA do
        if listA[i] ~= listB[i] then return false end
    end
    return true
end

return CommonUtils