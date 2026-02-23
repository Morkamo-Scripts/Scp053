using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scp053.Components.Features;

public static class Utils
{
    public static void ProceduralParticles(
            Player player,
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
            }
            
            Timing.RunCoroutine(SpawnParticleField(
                player, gameObject, particleColor, duration, spawnRate, fieldLocalScale, particleSize,
                intensity, appearSpeed, idleRotateSpeed, disappearSpeed));
        }
        
        private static IEnumerator<float> SpawnParticleField(
            Player player,
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
                if (player.IsEffectActive<Invisible>())
                {
                    yield return Timing.WaitForSeconds(1f);
                    continue;
                }
                
                yield return Timing.WaitForSeconds(spawnRate);

                // Локальные координаты (в пределах anchor.localScale / 2)
                Vector3 localOffset = new Vector3(
                    Random.Range(-localScale.x / 2f, localScale.x / 2f),
                    Random.Range(-localScale.y / 2f, localScale.y / 2f),
                    Random.Range(-localScale.z / 2f, localScale.z / 2f)
                );

                Vector3 spawnPos = anchor.transform.position + anchor.transform.rotation * localOffset;

                Primitive particle = Primitive.Create(PrimitiveType.Cube);
                particle.Base.syncInterval = 0;
                
                if (Room.Get(anchor.transform.position).Type == RoomType.Surface)
                {
                    particle.Color = particleColor with { a = 0.8f };
                }
                else
                {
                    particle.Color = (particleColor * intensity) with { a = 0.5f };
                }
                
                particle.Position = spawnPos;
                particle.Scale = Vector3.zero;
                particle.Visible = true;
                particle.IsStatic = false;
                particle.Collidable = false;

                Quaternion baseRotation = Random.rotation;
                particle.Rotation = baseRotation;
                particle.Spawn();

                float totalLife = (1f / appearSpeed) + (1f / disappearSpeed); // оценка общего времени

                Timing.RunCoroutine(ParticleLifeCycleHandler(
                    particle, particleSize, appearSpeed, idleRotateSpeed, disappearSpeed, baseRotation, totalLife));
            }

            Object.Destroy(anchor);
        }
        
        private static IEnumerator<float> ParticleLifeCycleHandler(
            Primitive particle,
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

            // Плавное появление
            while (time < appearTime && particle != null)
            {
                float t = time / appearTime;
                float scale = Mathf.Lerp(0f, maxScale, t);
                particle.Scale = Vector3.one * scale;
                particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * time, 0f);

                time += Time.deltaTime;
                yield return 0f;
            }

            time = 0f;

            // Idle состояние (вращение, scale остаётся)
            while (time < idleTime && particle != null)
            {
                particle.Scale = Vector3.one * maxScale;
                particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * (appearTime + time), 0f);

                time += Time.deltaTime;
                yield return 0f;
            }

            time = 0f;

            // Плавное исчезновение
            while (time < disappearTime && particle != null)
            {
                float t = time / disappearTime;
                float scale = Mathf.Lerp(maxScale, 0f, t);
                particle.Scale = Vector3.one * scale;
                particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * (appearTime + idleTime + time), 0f);

                time += Time.deltaTime;
                yield return 0f;
            }

            particle?.Destroy();
        }
}