local UpdateBeat = UpdateBeat
local max = math.max
local time,date = os.time,os.date

local Timer = class("Timer")
--region 全局设定
Timer.deltaTime = 0 --unity engine deltaTime
Timer.unscaledDeltaTime = 0 --unity engine unscaledDeltaTime
Timer.realDeltaTime = 0 --真实时间的DeltaTime
Timer.serverTimeDelta = 0 --与服务器的时间差
Timer.lastRealTime = time() --上一次时间

-- COUNT_MODE_FRAME
local COUNT_MODE_DELTA = 1 --使用引擎的deltaTime
local COUNT_MODE_UNSCALED = 2 --使用引擎的unscaledDeltaTime
local COUNT_MODE_REAL = 3 --使用真实的DeltaTime

function Timer.GetLocalTime()
    return time()
end

function Timer.SetServerTime(serverTime)
    Timer.serverTimeDelta = serverTime - time()
end

function Timer.CurrentTime()
    return time() + Timer.serverTimeDelta
end

function Timer.SetDeltaTime(deltaTime,unscaledDeltaTime)
    Timer.deltaTime = deltaTime
    Timer.unscaledDeltaTime = unscaledDeltaTime
    local oldRealTime = Timer.lastRealTime
    Timer.lastRealTime = Timer.CurrentTime()
    Timer.realDeltaTime = Timer.lastRealTime - oldRealTime
end

function Timer.GetRealDeltaTime()
    return Timer.realDeltaTime
end

function Timer.GetDeltaTime()
    return Timer.deltaTime
end

function Timer.GetUnscaledDeltaTime()
    return Timer.unscaledDeltaTime
end

function Timer.GetFrameDeltaTime() return 1 end

local function genTimer(duration,cb,caller,loop,countMod)
    local timer = Timer.new(cb,caller)
    timer:Set(duration,loop or -1,countMod)
    return timer
end

---循环，不填loop次数表示无限循环
function Timer.Loop(duration,cb,caller,loop)
    return genTimer(duration,cb,caller,loop,COUNT_MODE_DELTA)
end

---一次性时钟
function Timer.Once(duration,cb,caller)
    return genTimer(duration,cb,caller,1,COUNT_MODE_DELTA)
end

---unscaled循环，不填loop次数表示无限循环
function Timer.UnscaledLoop(duration,cb,caller,loop)
    return genTimer(duration,cb,caller,loop,COUNT_MODE_UNSCALED)
end

---一unscaled次性时钟
function Timer.UnscaledOnce(duration,cb,caller)
    return genTimer(duration,cb,caller,1,COUNT_MODE_UNSCALED)
end

function Timer.RealLoop(duration,cb,caller,loop)
    return genTimer(duration,cb,caller,loop,COUNT_MODE_REAL)
end

function Timer.RealOnce(duration,cb,caller)
    return genTimer(duration,cb,caller,1,COUNT_MODE_REAL)
end

---帧循环
function Timer.FrameLoop(duration,loop,cb,caller)
    return genTimer(duration,cb,caller,loop)
end
---帧一次性的
function Timer.FrameOnce(duration,cb,caller)
    return genTimer(duration,cb,caller,1)
end
--endregion

local nextId = 1
local timers = {}

function Timer:ctor(cb,caller)
    self.cb = cb
    self.caller = caller
    self.id = nextId
    timers[nextId] = self
    nextId = nextId + 1
end

function Timer:Set(duration,loop,countMod)
    self.duration = duration
    self.loop = loop or 1
    self.countMod = countMod
    if countMod == COUNT_MODE_DELTA then
        self.getDeltaTimeFunc = Timer.GetDeltaTime
    elseif countMod == COUNT_MODE_UNSCALED then
        self.getDeltaTimeFunc = Timer.GetUnscaledDeltaTime
    elseif countMod == COUNT_MODE_REAL then
        self.getDeltaTimeFunc = Timer.GetRealDeltaTime
    else
        self.getDeltaTimeFunc = Timer.GetFrameDeltaTime
    end
end


--delay为延迟，不填没有首次延迟
function Timer:Start(delay)
    if self:IsStopped() then return self end
    self.running = true
    if not self.handler then
        self.handler = UpdateBeat:RegisterListener(self.Update,self)
    end
    self.time = self.duration
    if delay then
        self.time = self.time + delay
    end
    return self
end

function Timer:Update()
    if not self.running then return end
    local delta = self.getDeltaTimeFunc()
    self:OnCd()
    self.time = self.time - delta
    while self.time <=0 do
        self:OnTime()
        if self.loop > 0 then
            self.loop = self.loop - 1
            self.time = self.time + self.duration
        end
        if self.loop == 0 then
            self:Stop()
            break
        elseif self.loop < 0 then
            self.time = self.time + self.duration
        end
    end
end

function Timer:OnTime()
    self.cb(self.caller)
end

function Timer:Stop()
    if not self.id then return end
    self.running = false
    if self.cb then
        self.cb = nil
        self.caller = nil
    end
    if self.onCd then
        self.onCd = nil
        self.onCdCaller = nil
    end
    if self.handler then
        UpdateBeat:RemoveListener(self.handler)
        self.handler = nil
    end
    timers[self.id] = nil
    self.id = nil
end

function Timer:Pause()
    self.running = false
end

function Timer:Resume(restart)
    if restart then
        self:Start()
    else
        self.running = true
    end
end

function Timer:IsStopped()
    return self.id == nil
end

--注册时钟cd，用于响应当前已持续时间
function Timer:RegisterCd(onCd,caller)
    self.onCd = onCd
    self.onCdCaller = caller
    return self
end

function Timer:OnCd()
    if self.onCd then
        local cd = max(0,self.duration-self.time)
        if self.onCdCaller then
            self.onCd(self.onCdCaller,cd)
        else
            self.onCd(cd)
        end
    end
end

_G.Timer = Timer