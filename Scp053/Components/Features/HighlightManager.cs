using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Scp053.Components.Features
{
    public static class HighlightManager
    {
        /// <summary>
        /// Подсветка объектов на прикреплении дочернего объекта типа Light
        /// </summary>
        /// <param name="gameObject">Цель подсветки</param>
        /// <param name="color">Цвет свечения</param>
        /// <param name="shadowsType">Тип ренденра теней</param>
        /// <param name="range">Радиус свечения</param>
        /// <param name="intensity">Интенсивность свечения</param>
        public static void HighlightObject(GameObject gameObject, Color color, LightShadows shadowsType = LightShadows.None, 
            float range = 3f, float intensity = 3f)
        {
            Vector3 position = gameObject.transform.position;

            Light lightObject = Light.Create(position: position, spawn: false);
            lightObject.Color = color;
            lightObject.Intensity = intensity;
            lightObject.Range = range;
            lightObject.ShadowType = shadowsType;
            lightObject.Position = position;
            lightObject.Base.transform.SetParent(gameObject.transform);
            lightObject.Spawn();
        }


        /// <summary>
        /// Динамически обновляемый трассер, следующий за заданным объектом
        /// </summary>
        /// <param name="gameObject">Объект трассировки.</param>
        /// <param name="traceColor">Цвет трассера.</param>
        /// <param name="trackingDuration">Указывает общее время трассировки объекта</param>
        /// <param name="glowIntensity">Интенсивность свечения трассера.</param>
        /// <param name="tracerDuration">Длительность показа трассера.</param>
        /// <param name="tracingAccuracy">Точность расчёта. Чем меньше значение, тем более точной будет линиия трассера. (Может повлиять на производительность)</param>
        /// <param name="alwaysTrace">Будет ли производиться трассировка когда объект неподвижен.</param>
        public static void DynamicTracing(GameObject gameObject, Color traceColor, float trackingDuration = 0, ushort glowIntensity = 50,
            float tracerDuration = 1.5f, float tracingAccuracy = 0.05f, bool alwaysTrace = false)
        {
            Timing.RunCoroutine(_dynamicTracing(gameObject, traceColor, trackingDuration, glowIntensity,
                tracerDuration, tracingAccuracy, alwaysTrace));
        }
        
        public static Light MakeLight(Vector3 position, Color color, LightShadows shadowType = LightShadows.Soft, float range = 10f, float intensity = 10f)
        {
            Light light = Light.Create(position: position, spawn: false);
            light.Color = color;
            light.ShadowType = shadowType;
            light.Intensity = intensity;
            light.Range = range;
            return light;
        }
        
        /// <summary>
        /// Производит быструю симуляцию динамического поведения света на основе 3-х фаз. (Появление -> Простой -> Затухание)
        /// </summary>
        /// <param name="light">Объект симуляции</param>
        /// <param name="peakIntensity">Пиковая интенсивность света</param>
        /// <param name="idleDuration">Время фазы простоя</param>
        /// <param name="turnLightDuration">Общее время на фазу зажигания</param>
        /// <param name="fadeLightDuration">Общее время на фазу затухания</param>
        public static void DynamicLight(Light light, float peakIntensity = 10000f,
            float idleDuration = 1f, float turnLightDuration = 0.1f, float fadeLightDuration = 0.1f)
        {
            Timing.RunCoroutine(_dynamicLight(light, peakIntensity, idleDuration, 
                turnLightDuration, fadeLightDuration));
        }

        /// <summary>
        /// Создаёт процедурное визуальное облако частиц вокруг заданного объекта. 
        /// </summary>
        /// <param name="gameObject">Игровой объект, к которому будет привязана невидимая зона спавна частиц.</param>
        /// <param name="particleColor">Цвет частицы. Может быть полупрозрачным, чтобы добиться нужного визуального эффекта.</param>
        /// <param name="duration">Общая продолжительность генерации частиц (в секундах).</param>
        /// <param name="spawnRate">Интервал между появлением новых частиц (в секундах). Чем меньше, тем плотнее облако.</param>
        /// <param name="fieldLocalScale">Размер невидимой зоны спавна частиц относительно объекта (локальный масштаб якоря). Default: (3, 3, 3).</param>
        /// <param name="particleSize">Максимальный размер одной частицы после появления.</param>
        /// <param name="intensity">Множитель яркости/насыщенности цвета частицы. Рекомендуется значение от 1 до 100.</param>
        /// <param name="appearSpeed">Скорость анимации появления частицы. Выражается в "ускорении" — чем больше значение, тем быстрее появление.</param>
        /// <param name="idleRotateSpeed">Скорость вращения частицы в стадии существования. Задаётся в градусах в секунду.</param>
        /// <param name="disappearSpeed">Скорость исчезновения частицы. Чем выше, тем быстрее частица "гаснет".</param>
        public static void ProceduralParticles(
            GameObject gameObject,
            Color particleColor,
            float duration = 0f,
            float spawnRate = 0.01f,
            Vector3 fieldLocalScale = default,
            float particleSize = 0.1f,
            ushort intensity = 80,
            float appearSpeed = 3f,
            float idleRotateSpeed = 30f,
            float disappearSpeed = 3f)
        
        {
            if (fieldLocalScale == default)
                fieldLocalScale = Vector3.one * 3f;
            
            switch (Room.Get(gameObject.transform.position).Zone)
            {
                case(ZoneType.LightContainment): intensity = (ushort)(intensity * 0.55f);
                    break;
                    
                case (ZoneType.HeavyContainment): intensity = (ushort)(intensity * 0.45f);
                    break;

                case (ZoneType.Surface): intensity = (ushort)(intensity * 1.5f);
                    break;
                
                case (ZoneType.Pocket): intensity = (ushort)(intensity * 0.3f);
                    break;
                
                default:
                    intensity = (ushort)(intensity * 0.45f);
                    break;
            }
            
            float idleTime = duration > 0f
                ? Mathf.Max(0.1f, duration * 0.6f)
                : 0.75f;

            Timing.RunCoroutine(SpawnParticleField(
                gameObject,
                particleColor,
                spawnRate,
                fieldLocalScale,
                particleSize,
                intensity,
                appearSpeed,
                idleTime,
                idleRotateSpeed,
                disappearSpeed
            ));
        }
        
        /// <summary>
        /// Создаёт процедурное облако частиц вокруг заданного объекта с динамическими источниками света на каждый GameObject.
        /// (ДОРОГОСТОЯЩАЯ ФУНКЦИЯ). 
        /// </summary>
        /// <param name="gameObject">Игровой объект, к которому будет привязана невидимая зона спавна частиц.</param>
        /// <param name="particleColor">Цвет частицы. Может быть полупрозрачным, чтобы добиться нужного визуального эффекта.</param>
        /// <param name="duration">Общая продолжительность генерации частиц (в секундах).</param>
        /// <param name="spawnRate">Интервал между появлением новых частиц (в секундах). Чем меньше, тем плотнее облако.</param>
        /// <param name="fieldLocalScale">Размер невидимой зоны спавна частиц относительно объекта (локальный масштаб якоря). Default: (3, 3, 3).</param>
        /// <param name="particleSize">Максимальный размер одной частицы после появления.</param>
        /// <param name="intensity">Множитель яркости/насыщенности цвета частицы. Рекомендуется значение от 1 до 100.</param>
        /// <param name="appearSpeed">Скорость анимации появления частицы. Выражается в "ускорении" — чем больше значение, тем быстрее появление.</param>
        /// <param name="idleRotateSpeed">Скорость вращения частицы в стадии существования. Задаётся в градусах в секунду.</param>
        /// <param name="disappearSpeed">Скорость исчезновения частицы. Чем выше, тем быстрее частица "гаснет".</param>
        public static void ProceduralParticlesWithLightTracing(
            GameObject gameObject,
            Color particleColor,
            float duration = 0f,
            float spawnRate = 0.01f,
            Vector3 fieldLocalScale = default,
            float particleSize = 0.1f,
            ushort intensity = 80,
            float appearSpeed = 3f,
            float idleRotateSpeed = 30f,
            float disappearSpeed = 3f)
        
        {
            if (fieldLocalScale == default)
                fieldLocalScale = Vector3.one * 3f;
            
            switch (Room.Get(gameObject.transform.position).Zone)
            {
                case(ZoneType.LightContainment): intensity = (ushort)(intensity * 0.8f);
                    break;
                    
                case (ZoneType.HeavyContainment): intensity = (ushort)(intensity * 0.6f);
                    break;

                case (ZoneType.Surface): intensity = (ushort)(intensity * 1.5f);
                    break;
            }
            
            Timing.RunCoroutine(SpawnParticleFieldWithLightTracing(
                gameObject, particleColor, duration, spawnRate, fieldLocalScale, particleSize,
                intensity, appearSpeed, idleRotateSpeed, disappearSpeed));
        }
        
        public static void ProceduralExplosionParticles(
            GameObject gameObject,
            Color particleColor,
            int count = 50,
            Vector3 fieldLocalScale = default,
            float particleSize = 0.1f,
            ushort intensity = 80,
            float appearSpeed = 5f,
            float flySpeed = 2f,
            float disappearSpeed = 5f)
        
        {
            if (fieldLocalScale == default)
                fieldLocalScale = Vector3.one * 3f;
            
            switch (Room.Get(gameObject.transform.position).Zone)
            {
                case(ZoneType.LightContainment): intensity = (ushort)(intensity * 0.55f);
                    break;
                    
                case (ZoneType.HeavyContainment): intensity = (ushort)(intensity * 0.45f);
                    break;

                case (ZoneType.Surface): intensity = (ushort)(intensity * 1.5f);
                    break;
                
                case (ZoneType.Pocket): intensity = (ushort)(intensity * 0.3f);
                    break;
                
                default:
                    intensity = (ushort)(intensity * 0.45f);
                    break;
            }

            Timing.RunCoroutine(SpawnExplosionParticles(
                gameObject, particleColor, count, fieldLocalScale, particleSize,
                intensity, appearSpeed, flySpeed, disappearSpeed));
        }


        #region IEnumerators
        
            private static IEnumerator<float> _dynamicTracing(
                GameObject gameObject, 
                Color traceColor, 
                float trackingDuration = 0, 
                ushort glowIntensity = 50, 
                float tracerDuration = 1.5f, 
                float tracingAccuracy = 0.05f, 
                bool alwaysTrace = false)
            
            {
                bool cycleEnded = false;
                Vector3 lastPos = gameObject.transform.position;

                if (trackingDuration > 0)
                {
                    Timing.CallDelayed(trackingDuration, () => 
                    {
                        cycleEnded = true;
                    });
                }
                
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (gameObject != null)
                {
                    if (cycleEnded)
                        yield break;
                    
                    yield return Timing.WaitForSeconds(tracingAccuracy);

                    Vector3 currentPos = gameObject.transform.position;
                    Vector3 direction = currentPos - lastPos;
                    float length = direction.magnitude;

                    if (!alwaysTrace && length < 0.01f)
                        continue;

                    Primitive tracer = Primitive.Create(PrimitiveType.Cylinder);
                    tracer.Base.syncInterval = 0;
                    
                    if (Room.Get(gameObject.transform.position).Type == RoomType.Surface)
                        tracer.Color = (traceColor * glowIntensity) with { a = 1f };
                    else
                        tracer.Color = (traceColor * glowIntensity) with { a = 0.1f };

                    tracer.Scale = new Vector3(0.02f, length / 2f, 0.02f);
                    tracer.Position = lastPos + direction * 0.5f;
                    tracer.Rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
                    tracer.Visible = true;
                    tracer.IsStatic = false;
                    tracer.Collidable = false;
                    tracer.Spawn();

                    Timing.CallDelayed(tracerDuration, () => { tracer.Destroy(); });

                    lastPos = currentPos;
                }
            }

            private static IEnumerator<float> _dynamicLight(
                Light light, 
                float peakIntensity = 10000f,
                float idleDuration = 1f, 
                float turnLightDuration = 0.1f, 
                float fadeLightDuration = 0.1f)
            
            {
                light.Intensity = 0;
                light.Spawn();
                
                for (float t = 0; t < turnLightDuration; t += 0.01f)
                {
                    light.Intensity = Mathf.Lerp(0f, peakIntensity, t / turnLightDuration);
                    yield return Timing.WaitForSeconds(0.01f);
                }

                light.Intensity = peakIntensity;

                yield return Timing.WaitForSeconds(idleDuration);

                for (float t = 0; t < fadeLightDuration; t += 0.01f)
                {
                    light.Intensity = Mathf.Lerp(peakIntensity, 0f, t / fadeLightDuration);
                    yield return Timing.WaitForSeconds(0.01f);
                }

                light.Intensity = 0f;
                light.UnSpawn();
            }

            #region ParticleField
                
                private static IEnumerator<float> SpawnParticleFieldWithLightTracing(
                GameObject playerObject,
                Color particleColor,
                float duration,
                float spawnRate,
                Vector3 localScale,
                float particleSize,
                ushort intensity,
                float appearSpeed,
                float idleRotateSpeed,
                float disappearSpeed)
            {
                GameObject anchor = new GameObject("ParticleAnchor");
                anchor.transform.SetParent(playerObject.transform);
                anchor.transform.localPosition = Vector3.zero;
                anchor.transform.localScale = localScale;

                bool ended = false;
                if (duration != 0)
                    Timing.CallDelayed(duration, () => ended = true);

                while (!ended && playerObject != null)
                {
                    yield return Timing.WaitForSeconds(spawnRate);

                    Vector3 localOffset = new Vector3(
                        Random.Range(-localScale.x / 2f, localScale.x / 2f),
                        Random.Range(-localScale.y / 2f, localScale.y / 2f),
                        Random.Range(-localScale.z / 2f, localScale.z / 2f)
                    );

                    Vector3 spawnPos = anchor.transform.position + anchor.transform.rotation * localOffset;

                    Primitive particle = Primitive.Create(PrimitiveType.Cube);
                    particle.Base.syncInterval = 0;

                    Room room = Room.Get(anchor.transform.position);
                    bool isSurface = room.Type == RoomType.Surface;

                    if (isSurface)
                        particle.Color = (particleColor * intensity) with { a = 1f };
                    else
                        particle.Color = (particleColor * intensity) with { a = 0.5f };

                    particle.Position = spawnPos;
                    particle.Scale = Vector3.zero;
                    particle.Visible = true;
                    particle.IsStatic = false;
                    particle.Collidable = false;

                    Quaternion baseRotation = Random.rotation;
                    particle.Rotation = baseRotation;
                    particle.Spawn();

                    float totalLife = (1f / appearSpeed) + (1f / disappearSpeed);

                    if (isSurface)
                    {
                        Light glow = Light.Create(position: spawnPos, spawn: false);
                        glow.Color = particleColor;
                        glow.Intensity = intensity;
                        glow.Range = particleSize * 6f;
                        glow.ShadowType = LightShadows.None;
                        glow.Position = spawnPos;
                        glow.Spawn();

                        Timing.RunCoroutine(ParticleLifeCycleHandlerWithLight(
                            particle, glow, particleSize, appearSpeed, idleRotateSpeed, disappearSpeed, baseRotation, totalLife));
                    }
                    else
                    {
                        var idleTime = 0.75f;
                        
                        Timing.RunCoroutine(ParticleLifeCycleHandler(
                            particle,
                            particleSize,
                            appearSpeed,
                            idleTime,
                            idleRotateSpeed,
                            disappearSpeed,
                            baseRotation
                        ));
                    }
                }

                Object.Destroy(anchor);
            }

            
                private static IEnumerator<float> ParticleLifeCycleHandlerWithLight(
                Primitive particle,
                Light light,
                float maxScale,
                float appearSpeed,
                float rotationSpeed,
                float disappearSpeed,
                Quaternion baseRotation,
                float estimatedLifetime)
            {
                float appearTime = 1f / appearSpeed;
                float disappearTime = 1f / disappearSpeed;
                float idleTime = estimatedLifetime - appearTime - disappearTime;

                float time = 0f;

                while (time < appearTime && particle != null)
                {
                    float t = time / appearTime;
                    float scale = Mathf.Lerp(0f, maxScale, t);
                    particle.Scale = Vector3.one * scale;
                    particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * time, 0f);

                    if (light != null)
                    {
                        light.Position = particle.Position;
                        light.Intensity = Mathf.Lerp(0f, maxScale * 25f, t);
                    }

                    time += Time.deltaTime;
                    yield return 0f;
                }

                time = 0f;

                while (time < idleTime && particle != null)
                {
                    particle.Scale = Vector3.one * maxScale;
                    particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * (appearTime + time), 0f);

                    if (light != null)
                    {
                        light.Position = particle.Position;
                        light.Intensity = maxScale * 25f;
                    }

                    time += Time.deltaTime;
                    yield return 0f;
                }

                time = 0f;

                while (time < disappearTime && particle != null)
                {
                    float t = time / disappearTime;
                    float scale = Mathf.Lerp(maxScale, 0f, t);
                    particle.Scale = Vector3.one * scale;
                    particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * (appearTime + idleTime + time), 0f);

                    if (light != null)
                    {
                        light.Position = particle.Position;
                        light.Intensity = Mathf.Lerp(maxScale * 25f, 0f, t);
                    }

                    time += Time.deltaTime;
                    yield return 0f;
                }

                if (light != null)
                    light.UnSpawn();

                particle?.Destroy();
            }

                #region DynamicPassiveParticles

                    // Обрабатывает основную логику появления частиц
                    private static IEnumerator<float> SpawnParticleField(
                        GameObject parentObject,
                        Color particleColor,
                        float spawnRate,
                        Vector3 localScale,
                        float particleSize,
                        ushort intensity,
                        float appearSpeed,
                        float idleTime,
                        float idleRotateSpeed,
                        float disappearSpeed)
                    {
                        GameObject anchor = new GameObject("ParticleAnchor");
                        anchor.transform.SetParent(parentObject.transform);
                        anchor.transform.localPosition = Vector3.zero;
                        anchor.transform.localScale = localScale;

                        // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                        while (parentObject != null)
                        {
                            yield return Timing.WaitForSeconds(spawnRate);

                            if (parentObject == null)
                                break;

                            Vector3 localOffset = new Vector3(
                                Random.Range(-localScale.x * 0.5f, localScale.x * 0.5f),
                                Random.Range(-localScale.y * 0.5f, localScale.y * 0.5f),
                                Random.Range(-localScale.z * 0.5f, localScale.z * 0.5f)
                            );

                            Vector3 spawnPos =
                                anchor.transform.position +
                                anchor.transform.rotation * localOffset;

                            Primitive particle = Primitive.Create(PrimitiveType.Cube);
                            particle.Base.syncInterval = 0;

                            bool isSurface =
                                Room.Get(anchor.transform.position)?.Type == RoomType.Surface;

                            particle.Color = (particleColor * intensity) with
                            {
                                a = isSurface ? 1f : 0.5f
                            };

                            particle.Position = spawnPos;
                            particle.Scale = Vector3.zero;
                            particle.Visible = true;
                            particle.Collidable = false;
                            particle.IsStatic = false;

                            Quaternion baseRotation = Random.rotation;
                            particle.Rotation = baseRotation;
                            particle.Spawn();

                            Timing.RunCoroutine(ParticleLifeCycleHandler(
                                particle,
                                particleSize,
                                appearSpeed,
                                idleTime,
                                idleRotateSpeed,
                                disappearSpeed,
                                baseRotation
                            ));
                        }

                        Object.Destroy(anchor);
                    }

                    // Обрабатывает второстепенное поведение частиц
                    private static IEnumerator<float> ParticleLifeCycleHandler(
                        Primitive particle,
                        float maxScale,
                        float appearSpeed,
                        float idleTime,
                        float rotationSpeed,
                        float disappearSpeed,
                        Quaternion baseRotation)
                    {
                        float time = 0f;

                        float appearDuration = 1f / appearSpeed;
                        float disappearDuration = 1f / disappearSpeed;

                        // --- APPEAR ---
                        while (time < appearDuration)
                        {
                            if (particle == null)
                                yield break;

                            float t = time / appearDuration;
                            particle.Scale = Vector3.one * Mathf.Lerp(0f, maxScale, t);
                            particle.Rotation =
                                baseRotation * Quaternion.Euler(0f, rotationSpeed * time, 0f);

                            time += Time.deltaTime;
                            yield return 0f;
                        }

                        // --- IDLE ---
                        time = 0f;
                        while (time < idleTime)
                        {
                            if (particle == null)
                                yield break;

                            particle.Scale = Vector3.one * maxScale;
                            particle.Rotation =
                                baseRotation * Quaternion.Euler(
                                    0f,
                                    rotationSpeed * (appearDuration + time),
                                    0f);

                            time += Time.deltaTime;
                            yield return 0f;
                        }

                        // --- DISAPPEAR ---
                        time = 0f;
                        while (time < disappearDuration)
                        {
                            if (particle == null)
                                yield break;

                            float t = time / disappearDuration;
                            particle.Scale = Vector3.one * Mathf.Lerp(maxScale, 0f, t);
                            particle.Rotation =
                                baseRotation * Quaternion.Euler(
                                    0f,
                                    rotationSpeed * (appearDuration + idleTime + time),
                                    0f);

                            time += Time.deltaTime;
                            yield return 0f;
                        }

                        particle?.Destroy();
                    }
                
                #endregion

                #region DynamicExplosionParticles

                    private static IEnumerator<float> SpawnExplosionParticles(
                        GameObject playerObject,
                        Color particleColor,
                        int particleCount,
                        Vector3 localScale,
                        float particleSize,
                        ushort intensity,
                        float appearSpeed,
                        float flySpeed,
                        float disappearSpeed)
                    {
                        if (playerObject == null)
                            yield break;

                        GameObject anchor = new GameObject("ExplosionAnchor");
                        anchor.transform.SetParent(playerObject.transform);
                        anchor.transform.localPosition = Vector3.zero;
                        anchor.transform.localScale = localScale;

                        for (int i = 0; i < particleCount; i++)
                        {
                            if (playerObject == null)
                                break;

                            Vector3 localOffset = new Vector3(
                                Random.Range(-localScale.x * 0.5f, localScale.x * 0.5f),
                                Random.Range(-localScale.y * 0.5f, localScale.y * 0.5f),
                                Random.Range(-localScale.z * 0.5f, localScale.z * 0.5f)
                            );

                            Vector3 worldOffset = anchor.transform.rotation * localOffset;
                            Vector3 spawnPos = anchor.transform.position + worldOffset;

                            Vector3 direction =
                                (worldOffset.normalized + Random.insideUnitSphere * 0.25f).normalized;

                            Primitive particle = Primitive.Create(PrimitiveType.Cube);
                            particle.Base.syncInterval = 0;

                            bool isSurface =
                                Room.Get(anchor.transform.position)?.Type == RoomType.Surface;

                            particle.Color = (particleColor * intensity) with
                            {
                                a = isSurface ? 1f : 0.5f
                            };

                            particle.Position = spawnPos;
                            particle.Scale = Vector3.zero;
                            particle.Visible = true;
                            particle.IsStatic = false;
                            particle.Collidable = false;

                            Quaternion baseRotation = Random.rotation;
                            particle.Rotation = baseRotation;
                            particle.Spawn();

                            Timing.RunCoroutine(ExplosionParticleLifecycle(
                                particle,
                                direction,
                                particleSize,
                                appearSpeed,
                                flySpeed,
                                disappearSpeed,
                                baseRotation
                            ));
                        }

                        Object.Destroy(anchor);
                    }
                    
                    private static IEnumerator<float> ExplosionParticleLifecycle(
                        Primitive particle,
                        Vector3 direction,
                        float maxScale,
                        float appearSpeed,
                        float flySpeed,
                        float disappearSpeed,
                        Quaternion baseRotation)
                    {
                        float appearDuration = 1f / appearSpeed;
                        float disappearDuration = 1f / disappearSpeed;

                        float time = 0f;
                        
                        while (time < appearDuration)
                        {
                            if (particle == null)
                                yield break;

                            float t = time / appearDuration;
                            particle.Scale = Vector3.one * Mathf.Lerp(0f, maxScale, t);
                            particle.Position += direction * flySpeed * Time.deltaTime;
                            particle.Rotation =
                                baseRotation * Quaternion.Euler(0f, 60f * time, 0f);

                            time += Time.deltaTime;
                            yield return 0f;
                        }
                        
                        time = 0f;
                        while (time < disappearDuration)
                        {
                            if (particle == null)
                                yield break;

                            float t = time / disappearDuration;
                            particle.Scale = Vector3.one * Mathf.Lerp(maxScale, 0f, t);
                            particle.Position += direction * flySpeed * 0.75f * Time.deltaTime;
                            particle.Rotation =
                                baseRotation * Quaternion.Euler(
                                    0f,
                                    60f * (appearDuration + time),
                                    0f);

                            time += Time.deltaTime;
                            yield return 0f;
                        }

                        particle?.Destroy();
                    }

                #endregion
            
            #endregion

        #endregion
    }
}