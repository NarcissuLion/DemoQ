local Notifier = {}

local listeners = {}
local lookupTbl = {}
local __AUTO_INC_ID = 1

-- 添加监听，静态回调函数p_this传nil，如果p_callback是匿名函数可以通过返回值接收监听ID以便之后移除监听
function Notifier.AddListener(p_noti, p_callback, p_this)
    local callbacks = listeners[p_noti]
    if callbacks == nil then
        callbacks = {}
    end
    table.insert(callbacks, {id=__AUTO_INC_ID, noti=p_noti, func=p_callback, owner=p_this})
    listeners[p_noti] = callbacks
    lookupTbl[__AUTO_INC_ID] = callbacks[#callbacks]
    __AUTO_INC_ID = __AUTO_INC_ID + 1
    return __AUTO_INC_ID - 1
end

-- 非匿名函数直接移除监听
function Notifier.RemoveListener(p_noti, p_callback, p_this)
    local callbacks = listeners[p_noti]
    if callbacks == nil then
        return
    end
    for idx,callback in ipairs(callbacks) do
        if callback.func == p_callback and callback.owner == p_this then
            lookupTbl[callback.id] = nil
            table.remove(callbacks, idx)
            break
        end
    end
    listeners[p_noti] = callbacks
end

-- 匿名函数可以通过监听ID移除监听
function Notifier.RemoveListenerByID(p_ID)
    if lookupTbl[p_ID] == nil then
        return
    end
    Notifier.RemoveListener(lookupTbl[p_ID].noti, lookupTbl[p_ID].func, lookupTbl[p_ID].owner)
end

-- 触发消息，根据添加监听的顺序调用
function Notifier.Dispatch(notiName, ...)
    local callbacks = listeners[notiName]
    if callbacks == nil then
        return
    end

    local param = {...}
    for _,callback in ipairs(callbacks) do
        if callback.owner == nil then
            callback.func(param[1],param[2],param[3],param[4],param[5])
        else
            callback.func(callback.owner, param[1],param[2],param[3],param[4],param[5])
        end
    end
end

return Notifier