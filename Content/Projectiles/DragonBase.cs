using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles
{
    public class MinionPlayer : ModPlayer
    {
        public bool DragonMinion;
        public override void ResetEffects()
        {
            DragonMinion = false;
        }
    }
    public abstract class SimulateStarDustDragon : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;//正方碰撞箱
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }
        public Player Player => Main.player[Projectile.owner];
        public int itemDamage;//作为实时更新伤害的基数，传入武器基础面板
        public int buffType;//给玩家添加buff的type
        public int tail;//用来存尾体节的whoami
        public virtual float Offset => 1f;//设置体节位置时的偏移率
        public abstract bool ModPlayerMinionBool { get; set; }//继承了ModPlayer类中对应的召唤物bool属性
        public List<int> proj = new();//弹幕list，存入弹幕WhoAmI，不计尾体节
        public List<(Vector2 pos, float rot)> data = new();//数据list，计入尾体节，故count是proj.Count + 1
        public void BaseAI()
        {
            //玩家死了干掉弹幕
            if (Player.dead)
            {
                ModPlayerMinionBool = false;
            }
            //有buff保持弹幕,buff中会把这个bool设为true，后面会写到
            if (ModPlayerMinionBool)
            {
                Projectile.timeLeft = 2;
            }
            Player.AddBuff(buffType, 2);//给玩家上对应buff
        }
        public virtual void ActionAI()
        {
            StarDustDragonAI(Projectile, Player);
            for (int i = 0; i < proj.Count; i++)
            {
                if (!Main.projectile[proj[i]].active)
                {
                    proj.RemoveAt(i);
                    data.RemoveAt(i + 1);
                    i--;
                }
            }
        }//参数是身体体节数量，返回伤害倍率。这里是每节增加的伤害衰减10%，最多衰减5次，即从召唤第6节开始每多一节增伤约60%
        public virtual double ModifyDamageMult(int count)
        {
            return count * Math.Pow(0.9f, (count - 1) > 5 ? 5 : (count - 1));
        }
        public static (Vector2 pos, float rot) CalculatePosAndRot((Vector2 pos, float rot) tar, (Vector2 pos, float rot) me, float dis)
        {
            Vector2 chaseDir = tar.pos - me.pos;//当前的位置到目标的位置
            if (chaseDir == Vector2.Zero)//如果这两坐标怼一起了就分开，防止位置变成Nan
            {
                chaseDir = Vector2.One;
            }
            chaseDir = Vector2.Normalize(chaseDir);//向量单位化
            float chaserot = tar.rot - me.rot;//目标角度的和当前的角度差
            if (chaserot != 0)//角度即弹幕的运动（视觉上）方向，当方向不同，就每帧修正这个方向，修正值是差值的10%
            {
                chaseDir = chaseDir.RotatedBy(MathHelper.WrapAngle(chaserot) * 0.1f);
            }
            Vector2 center = tar.pos - chaseDir * dis;//让目标位置减去修正后算上距离的追击单位向量，即是下一帧应在的位置
            return (center, chaseDir.ToRotation());//返回应在位置和修正后角度
        }
        //用于设置体节数据
        public static void SetSection(int whoami, (Vector2 pos, float rot) data, double damage = 0)
        {
            Projectile p = Main.projectile[whoami];
            p.Center = data.pos;
            p.rotation = data.rot;
            p.timeLeft = 2;//保证弹幕存活，且在逻辑弹幕被右键buff取消后马上死亡
            p.originalDamage = p.damage = (int)damage;//给体节设置伤害，用于如果你们想让体节射出弹幕进行攻击之类的时候
        }
        public void SetProjSection(int whoami, (Vector2 pos, float rot) data, double damage = 0)
        {
            SetSection(whoami, data, damage); //直接复用现有方法
        }
        public override void AI()
        {
            ActionAI();//运动AI
            BaseAI();//基础AI
            //维护弹幕列表（移除无效体节）
            if (proj.Count > 0 && Main.projectile[proj[0]].active)
            {
                Projectile head = Main.projectile[proj[0]];
                head.Center = Projectile.Center;//同步逻辑弹幕位置到头部
                head.velocity = Projectile.velocity; //同步速度
                head.rotation = Projectile.rotation; //同步旋转角度
                head.netUpdate = true; //确保多人同步
            }
            //计算伤害
            Projectile.originalDamage = (int)(ModifyDamageMult(proj.Count - 1) * Player.GetDamage(DamageClass.Summon).ApplyTo(itemDamage));
            Projectile.rotation = Projectile.velocity.ToRotation();
            //更新数据[0]为头部位置
            data[0] = (Main.projectile[proj[0]].Center, Main.projectile[proj[0]].rotation);
            //设置头部数据
            SetProjSection(proj[0], data[0], Projectile.originalDamage);
            //计算后续体节位置
            for (int i = 1; i <= proj.Count; i++)
            {
                data[i] = CalculatePosAndRot(data[i - 1], data[i], Projectile.width * Projectile.scale * Offset);
                if (i < proj.Count)
                {
                    SetProjSection(proj[i], data[i], Projectile.originalDamage);
                }
            }
            SetProjSection(tail, data[proj.Count], Projectile.originalDamage); //设置尾部
        }/*
        public override void AI()
        {
            BaseAI();//基础AI
            ActionAI();//运动AI
            //维护弹幕列表和数据列表，在其内有弹幕死亡（召唤栏突然减少）时剔除元素
            for (int i = 0; i < proj.Count; i++)
            {
                if (!Main.projectile[proj[i]].active)
                {
                    proj.RemoveAt(i);
                    data.RemoveAt(i + 1);
                    i--;
                }
            }
            //设置伤害
            Projectile.originalDamage = (int)(ModifyDamageMult(proj.Count - 1) * Player.GetDamage(DamageClass.Summon).ApplyTo(itemDamage));
            //防止你自己写运动逻辑并忘了写这个
            Projectile.rotation = Projectile.velocity.ToRotation();
            //更新数据[0]，是逻辑弹幕的中心与角度
            data[0] = (Projectile.Center, Projectile.rotation);
            //设置头体节的数据
            SetProjSection(proj[0], data[0], Projectile.originalDamage);
            //重新计算data中的位置与角度，第一个数据是逻辑弹幕中心与角度，不需重新计算
            //proj未计入尾体节，但data有，所以这里是从1开始且 <= proj.Count
            for (int i = 1; i <= proj.Count; i++)
            {
                //注意，calculate方法的第三个参数距离，传入弹幕的 碰撞箱宽 * 缩放 * 继承后可重写的偏移率
                data[i] = CalculatePosAndRot(data[i - 1], data[i], Projectile.width * Projectile.scale * Offset);
                if (i < proj.Count)//proj中没有尾，是 <
                {
                    //设置身体节的数据
                    SetProjSection(proj[i], data[i], Projectile.originalDamage);
                }
            }
            //设置尾体节的数据, 逻辑弹幕的ai[1]是尾体节的WhoAmI
            SetProjSection((int)Projectile.ai[1], data[proj.Count], Projectile.originalDamage);
        }*/
        //弄个方便的方法，传入中心坐标和宽度，返回宽高等于传入宽度且中心是传入的坐标的矩形（碰撞箱）
        public static Rectangle RecCenter(Vector2 center, int Size)
        {
            return new Rectangle((int)center.X - Size / 2, (int)center.Y - Size / 2, Size, Size);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool intersects = false;
            for (int i = 0; i <= proj.Count; i++)//是 <= 哦
            {
                if (targetHitbox.Intersects(RecCenter(data[i].pos, projHitbox.Width)))//这里的碰撞箱已经应用了scale，所以直接拿他的宽
                {
                    intersects = true;
                    break;
                }
            }
            return intersects;
        }//这个是用来方便设置的方法，因为player.SpawnMinionOnCursor不像NewProjectile方法一样能直接写ai01。另外加上了在这里设置弹幕召唤栏位占用
        public static int SpawnMinion(Player player, IEntitySource source, int type, int damage, float kb, float ai0 = 0, float ai1 = 0, float minionSlots = 0)
        {
            int proj = player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, kb);
            Main.projectile[proj].ai[0] = ai0;
            Main.projectile[proj].ai[1] = ai1;
            Main.projectile[proj].minionSlots = minionSlots;
            return proj;//返回弹幕索引
        }
        public static void SummonSet<T>(Player player, IEntitySource source, int damage, float kb, int amount, int Logic, int Head, int Body, int Tail, int v) where T : SimulateStarDustDragon
        {
            //寻找是否有属于玩家的这类逻辑弹幕
            int logic = -1;
            foreach (Projectile p in Main.projectile)
            {
                if (p.type == Logic && p.active && p.owner == player.whoAmI)
                {
                    logic = p.whoAmI;
                    break;
                }
            }
            if (logic == -1)//没找到就发射逻辑弹幕和头身尾
            {
                //发射逻辑弹幕
                int L = SpawnMinion(player, source, Logic, damage, kb, 0, 0);
                if (Main.projectile[L].ModProjectile is T Proj)//使用模式匹配强制转换，以获取逻辑弹幕类里的字段属性
                {
                    Proj.itemDamage = damage;//传入基础伤害
                    var proj = Proj.proj;//获取逻辑弹幕类里的两个list
                    var data = Proj.data;
                    //首先，向数据list添加元素。数量是要召唤的体节量+2（一个头一个尾）
                    //至于这里加了个偏移是为了让之前那个目标位置-当前位置不为零
                    for (int i = 0; i < amount + 2; i++) data.Add((Main.MouseWorld + Vector2.One * i, 0));
                    //生成头体节
                    int p = SpawnMinion(player, source, Head, damage, kb);
                    proj.Add(p);//把头体节WhoAmI Add到proj列表
                    for (int i = 0; i < amount; i++)
                    {
                        //按照传入的单次生成身体数召唤身体，每次使用物品占用一个召唤栏
                        //所以这里把单个体节占用的召唤栏设为 1f / amount 。放心，弹幕的召唤栏位属性是float（比如双子眼召唤物）
                        int body = SpawnMinion(player, source, Body, damage, kb, i, 0, 1f / amount);
                        proj.Add(body);//把身体节WhoAmI Add到proj列表
                    }
                    //尾体节不需加入proj列表
                    //但要把逻辑弹幕的tail设为尾体节的WhoAmI用于之后设置它的数据
                    Proj.tail = SpawnMinion(player, source, Tail, damage, kb);
                }
            }
            else//有逻辑弹幕（那也就是有头有尾了）
            {
                if (Main.projectile[logic].ModProjectile is T Proj)//强转
                {
                    for (int i = 0; i < amount; i++)
                    {
                        //先Add数据列表
                        Proj.data.Add((Main.MouseWorld + Vector2.One * i, 0));
                        //再Add弹幕列表
                        Proj.proj.Add(SpawnMinion(player, source, Body, 0, 0, i, 0, 1f / amount));
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;//关闭原版绘制
        }
        public static void DrawSet(SpriteBatch spb, Projectile proj, int Body, int amount, Color color, float rot)
        {
            Texture2D tex = TextureAssets.Projectile[proj.type].Value;
            bool body = false;
            Rectangle rec = new();
            if (proj.type == Body)//给身体体节做特判
            {
                body = true;
                //根据身体的ai[0]来裁剪贴图，这里是纵向裁剪，有需要你们可以写个横向重载
                rec = new Rectangle(0, (int)proj.ai[0] * tex.Height / amount, (int)(tex.Width * 1f), tex.Height / amount);
            }
            spb.Draw(tex, proj.Center - Main.screenPosition, !body ? null : rec, color,
                proj.rotation + rot, (!body ? tex.Size() : rec.Size()) / 2f, proj.scale * 1f,
                Math.Abs(proj.rotation + rot) < Math.PI / 2f ? 0 : SpriteEffects.FlipVertically, 0);
            //瞧见这里的rotation没，所以在SetPosAndDmg函数里又写了一次设置rotation，这可是很重要的
        }
        #region 星尘龙AI
        public static void StarDustDragonAI(Projectile Projectile, Player Player,
        float MaxDisToPlayer = 2000, float SearchDis = 700,
        float MaxSpeedNoTarget = 15f, float MaxSpeedHasTarget = 30f)
        {
            //超出距离回归玩家
            if (Vector2.Distance(Player.Center, Projectile.Center) > MaxDisToPlayer)
            {
                Projectile.Center = Player.Center;
                Projectile.netUpdate = true;
            }
            //索敌逻辑
            int TargetWho = -1;
            NPC Target = Projectile.OwnerMinionAttackTargetNPC;
            if (Target != null && Target.CanBeChasedBy() &&
                Projectile.Distance(Target.Center) < SearchDis * 2f)
            {
                TargetWho = Target.whoAmI;
            }
            else
            {
                foreach (NPC Npc in Main.npc)
                {
                    if (Npc.CanBeChasedBy() && Player.Distance(Npc.Center) < 1000f &&
                        Projectile.Distance(Npc.Center) < SearchDis)
                    {
                        TargetWho = Npc.whoAmI;
                        break;
                    }
                }
            }
            //有目标时的追击逻辑
            if (TargetWho != -1)
            {
                NPC Npc = Main.npc[TargetWho];
                Vector2 TargetDir = Npc.Center - Projectile.Center;
                float Distance = TargetDir.Length();
                float SpeedFactor = 0.4f;
                //动态速度调整（越近越快）
                if (Distance < 600f) SpeedFactor = 0.6f;
                if (Distance < 300f) SpeedFactor = 0.8f;
                //追击方向修正
                if (Distance > Npc.Size.Length() * 0.75f)
                {
                    Vector2 NormalizedDir = Vector2.Normalize(TargetDir);
                    Projectile.velocity += NormalizedDir * SpeedFactor * 1.5f;
                    //方向偏差过大时减速
                    if (Vector2.Dot(Projectile.velocity, TargetDir) < 0.25f)
                        Projectile.velocity *= 0.8f;
                }
                //统一速度限制（修复原分支错误）
                if (Projectile.velocity.Length() > MaxSpeedHasTarget)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeedHasTarget;
            }
            //无目标时的游荡逻辑（独立分支）
            else
            {
                float DistanceToPlayer = Vector2.Distance(Player.Center, Projectile.Center);
                float WanderSpeed = 0.2f;
                //动态速度：离玩家越近速度越慢
                if (DistanceToPlayer < 200f) WanderSpeed = 0.12f;
                if (DistanceToPlayer < 140f) WanderSpeed = 0.06f;
                //向玩家缓慢靠近
                if (DistanceToPlayer > 100f)
                {
                    Vector2 MoveDir = Player.Center - Projectile.Center;
                    MoveDir.Normalize();
                    Projectile.velocity += MoveDir * WanderSpeed;
                }
                //近距离时添加随机游荡
                else
                {
                    //随机方向变化（每60帧触发）
                    if (Main.rand.NextBool(60))
                    {
                        float Angle = Main.rand.NextFloat(-0.5f, 0.5f);
                        Projectile.velocity = Projectile.velocity.RotatedBy(Angle);
                    }
                    //自然减速
                    Projectile.velocity *= 0.98f;
                }
                //限制最大游荡速度
                if (Projectile.velocity.Length() > MaxSpeedNoTarget)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeedNoTarget;
            }
            //统一更新旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        /*
        public static void StarDustDragonAI(Projectile Projectile, Player Player, float MaxDisToPlayer = 2000,
        float SearchDis = 700, float MaxSpeed_NoTarget = 15f, float MaxSpeed_HasTarget = 30f)
        //第一个是弹幕，第二个是玩家，第三个是离玩家的最大距离，第四个是索敌距离（不要超过1000），第五个是无目标限速，最后一个是有目标限速
        {
            //弹幕超出玩家2000f自动回归，并同步数据
            if (Vector2.Distance(Player.Center, Projectile.Center) > MaxDisToPlayer)
            {
                Projectile.Center = Player.Center;
                Projectile.netUpdate = true;
            }
            int TarWho = -1;//目标whoAmI
            NPC target = Projectile.OwnerMinionAttackTargetNPC;//召唤物自动索敌
            if (target != null && target.CanBeChasedBy())
            {
                //虽然但是，为什么要用 两倍 索敌距离
                if (Projectile.Distance(target.Center) < SearchDis * 2f)
                {
                    TarWho = target.whoAmI;
                }
            }
            //如果自动索敌没有找到目标，就搜索离玩家1000f内且在弹幕索敌距离内的目标
            if (TarWho < 0)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy() && Player.Distance(npc.Center) < 1000f)
                    {
                        if (Projectile.Distance(npc.Center) < SearchDis)
                        {
                            TarWho = npc.whoAmI;
                        }
                    }
                }
            }
            if (TarWho != -1)//有攻击目标
            {
                NPC npc = Main.npc[TarWho];
                Vector2 tarVel = npc.Center - Projectile.Center;
                float speed = 0.4f;//追击速度系数
                float dis = tarVel.Length();
                //越近越快
                if (dis < 600f)
                {
                    speed = 0.6f;
                }
                else if (dis < 300f)
                {
                    speed = 0.8f;
                }
                //到npc的距离比npc的碰撞箱大小的0.75倍大，也就是说离NPC还比较远
                //这使得星尘龙在穿过NPC后才会调头，而不是直接粘在它身上鬼畜
                else if (dis > npc.Size.Length() * 0.75f)
                {
                    //弹幕速度加上朝着npc的单位向量*追击速度系数*1.5f
                    Projectile.velocity += Vector2.Normalize(tarVel) * speed * 1.5f;
                    //如果追踪方向和速度方向夹角过大，减速(向量点乘你们自己研究，绝对值越大两向量夹角越大，范围是正负1)
                    if (Vector2.Dot(Projectile.velocity, tarVel) < 0.25f)
                    {
                        Projectile.velocity *= 0.8f;
                    }
                }
                //限制最大速度
                else if (Projectile.velocity.Length() > MaxSpeed_HasTarget)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed_HasTarget;
                }
                else//无攻击目标
                {
                    speed = 0.2f;//游荡速度系数
                    dis = Player.Center.Distance(Projectile.Center);//弹幕到玩家距离，越近越慢
                    if (dis < 200f)
                    {
                        speed = 0.12f;
                    }
                    if (dis < 140f)
                    {
                        speed = 0.06f;
                    }
                    if (dis > 100f)//在100f外朝着玩家飞
                    {
                        //abs绝对值，sign正负
                        if (Math.Abs(Player.Center.X - Projectile.Center.X) > 20f)//离玩家水平距离大于20，就给朝向玩家的水平速度
                        {
                            Projectile.velocity.X += speed * Math.Sign(Player.Center.X - Projectile.Center.X);
                        }
                        if (Math.Abs(Player.Center.Y - Projectile.Center.Y) > 10f)//离玩家垂直距离大于10，就给朝向玩家的垂直速度
                        {
                            Projectile.velocity.Y += speed * Math.Sign(Player.Center.Y - Projectile.Center.Y);
                        }
                    }
                    else if (Projectile.velocity.Length() > 2f)//在100f内且速度较大就减速
                    {
                        Projectile.velocity *= 0.96f;
                    }
                    if (Math.Abs(Projectile.velocity.Y) < 1f)//没啥速度了就慢慢向上游动
                    {
                        Projectile.velocity.Y -= 0.1f;
                    }
                    //无目标最大速度15f,有目标30f
                    if (Projectile.velocity.Length() > MaxSpeed_NoTarget)
                    {
                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed_NoTarget;
                    }
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }*/
        #endregion
    }
}