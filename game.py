import os

import discord
from discord.ext.commands import Bot
from dotenv import load_dotenv

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')
GUILD = os.getenv('DISCORD_GUILD')

# Discord Related Functions

def discordAction(player):
    msg = discord.Embed(title="**Hello " + player.name + "!**",
                        description="Please select an action!")

# Bot commands

bot = Bot(command_prefix = '!')

# On CLI side
@bot.event
async def on_ready():
    for guild in bot.guilds:
        if guild.name == GUILD:
            break

    print(
        f'{bot.user} is connected to the following guild:\n'
        f'{guild.name}(id: {guild.id})'
    )

# !play
@bot.command()
async def play(ctx):
    # Say intro message
    intro_Message = discord.Embed(title="**Hello! Welcome to the test monster killing game!**",
                    description="Please select an action!")
    sentMessage = await ctx.send(embed=intro_Message)

    emojis_to_actions = {
        "⚔": "attack",
        "🛡": "defend",
        "💖": "heal"
    }

    # Display possible actions as emojis
    for k in emojis_to_actions.keys():
        await sentMessage.add_reaction(k)

    # Wait for selection
    reaction, user = await bot.wait_for('reaction_add')

    # Unpack reaction to be used into dictionary
    emoji = str(reaction)
    
    # Extract emoji selection to keyword
    action = emojis_to_actions[emoji]

bot.run(TOKEN)


# Game Related Classes and Functions
MAX_PLAYERS = 2
MAX_MONSTERS = 5

MONSTER_HP = 10

PLAYER_HP = 10

TIMELIMIT = 5

class PlayerAction():
    def attack(self, p1, p2, amount):
        if p2.isDefending():
            p2.loseHP(amount // 2)
        else:
            p2.loseHP(amount)

    def defend(self, p1):
        p1.switchDefense()

    def heal(self, p1, amount):
        p1.gainHP(amount)

class Monster:
    def __init__(self, hp=MONSTER_HP):
        self.hp = hp
        self.points = 0

    def gainHP(self, amount):
        self.hp += amount
        return self.hp
    
    def loseHP(self, amount):
        self.hp -= amount
        return self.hp

class Player:
    def __init__(self, hp=PLAYER_HP, name="Player 1"):
        self.hp = hp
        self.points = 0
        self.name = name
        self.defend = False
        self.monsters = []

    def isDefending(self):
        return self.defend

    def getHP(self):
        return self.hp

    def getName(self):
        return self.name

    def switchDefense(self):
        self.defend = not self.defend

    def gainHP(self, amount):
        self.hp += amount
        return self.hp
    
    def loseHP(self, amount):
        self.hp -= amount
        return self.hp

    def noMonsters(self):
        return not self.monsters

    def spawnMonster(self):
        self.monsters.append(Monster())


class Game:
    def __init__(self):
        self.players = []
        self.monsters = []
        self.turn = 0
    
    def take_turn(self):
        if self.num_players < 1:
            print("Error: No players have joined the game")
    
    def doAction(self, action="Oops!"):
        print(action)

    def start(self):
        timelimit = TIMELIMIT

        # For now, it's single player
        self.players.append(Player(name="Chicken Dinner"))

        # While time allows, keep playing
        while timelimit > 0:
            # Players do actions on even turns
            if self.turn % 2 == 0:
                for player in self.players:
                    if player.noMonsters():
                        player.spawnMonster()
                    if player.getHP() <= 0:
                        break
                    phrase, amount = discordAction(player)







# class Unit:
#     def __init__(self):
#         #self.hp = 
#         # Every unit will have hp and mana

# class Player(Unit):
#     def __init__(self):
#         # type of player [Person, Remote]
#         # 

#     # action
#         # attack
#         # defend
#         # heal

# # Remote players will still be players
# # The way the game receives data will be different

# class Monster(Unit):
#     def __init__(self):