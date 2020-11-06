## Core Changes for Tab Targeting Refinements

### BaseCharacterEntity_SkillFunctions.cs
Line 289: Vector3 aimPosition = GetDefaultAttackAimPositionForSkill(damageInfo, isLeftHand);


### Damage.cs
Line 238: bool hasSelectedTarget = attacker.TryGetCastingTargetEntity(out selectedTarget);
Line 337: if (!attacker.TryGetCastingTargetEntity(out tempDamageableHitBox))

### Skill.cs
Line 158: if (skillUser.TryGetCastingTargetEntity(out targetEntity) && !targetEntity.IsDead())