# import os

# import discord
# from discord.ext.commands import Bot
# from dotenv import load_dotenv

# load_dotenv()
# TOKEN = os.getenv('DISCORD_TOKEN')
# GUILD = os.getenv('DISCORD_GUILD')

# Discord Related Functions

# async def discordAction(ctx, player):
#     msg = discord.Embed(title="**Hello " + player.name + "!**",
#                         description="Please select an action!")

#     # Say intro message
#     sentMessage = await ctx.send(embed=msg)

#     emojis_to_actions = {
#         "⚔": "attack",
#         "🛡": "defend",
#         "💖": "heal"
#     }

#     # Display possible actions as emojis
#     for k in emojis_to_actions.keys():
#         await sentMessage.add_reaction(k)

#     # Wait for selection
#     reaction, user = await bot.wait_for('reaction_add')

#     # Unpack reaction to be used into dictionary
#     emoji = str(reaction)
    
#     # Extract emoji selection to keyword
#     action = emojis_to_actions[emoji]

# # Bot commands

# bot = Bot(command_prefix = '!')

# # On CLI side
# @bot.event
# async def on_ready():
#     for guild in bot.guilds:
#         if guild.name == GUILD:
#             break

#     print(
#         f'{bot.user} is connected to the following guild:\n'
#         f'{guild.name}(id: {guild.id})'
#     )

import numpy as np

# Display Functions

def printIntro(players):
    names = ""
    for player in players:
        names += player.getName()
        if player != players[-1]:
            names += ", "
    intro_msg = "Hello " + names + "!" + "\n" + \
                "Welcome to the monster killing game!\n"
    print(intro_msg)

def printPlayerStats(player):
    msg = "Currently you have " + str(player.getHP()) + " HP!"
    print(msg)

def printMonsterStats(monster, index):
    msg = "Monster " + str(index) + " has " + str(monster.getHP()) + " HP!"
    print(msg)

def printPlayerChoice():
    while True:
        prompt = "What would you like to do?\n" + \
                "attack, defend, or heal?"
        print(prompt)
        response = input()
        print("")
        if (response == 'attack' or response == 'defend' or response == 'heal'):
            return response
        else:
            print("Error: The response is invalid...")
    return ""

def printLostHP(name, amount, hp):
    msg = name + " lost " + str(amount) + " HP.\n"
    msg += name + " now has " + str(hp) + " HP!"
    print(msg)
    if hp == 0:
        msg = name + " has died!"
        print(msg)

def printSelectMonster(monsters):
    num_monsters = len(monsters)
    if (num_monsters == 1):
        msg = "There is " + str(num_monsters) + " monster, choose one to target:"
    else:
        msg = "There are " + str(num_monsters) + " monsters, choose one to target:"
    print(msg)

    while True:
        # Display monster stats
        for i in range(num_monsters):
            printMonsterStats(monsters[i], i)
        
        # Pick from: 0, 1, 2, 3, ....
        msg = "Pick from: "
        for i in range(num_monsters):
            msg += str(i)
            if (i != num_monsters-1):
                msg += ", "
        print(msg)

        response = int(input())
        print("")
        if response in range(num_monsters):
            return response
        else:
            print("Error: The response is invalid...")
    return -1

def printDefend(name):
    msg = name + " defended!"
    print(msg)

def printHeal(name, amount, hp):
    msg = name + " healed " + str(amount) + " HP.\n"
    msg += name + " has " + str(hp) + " HP!"
    print(msg)

def printAttackPlayer(monster_name, player_name):
    msg = monster_name + " attacked " + player_name + "!"
    print(msg)

def printScores(player):
    msg = "Player " + player.getName() + " has " + str(player.getPoints()) + " points!"
    print(msg)

def printWinner(player):
    msg = "Player " + player.getName() + " won with " + str(player.getPoints()) + " points!"
    print(msg)

# Game Related Classes and Functions
MAX_PLAYERS = 2
MAX_MONSTERS = 5

MONSTER_HP = 10

PLAYER_HP = 10

TIMELIMIT = 5

# def attack(self, p1, p2, amount):
#     p2.loseHP(amount)

# def defend(self, p1):
#     p1.switchDefense()

# def heal(self, p1, amount):
#     p1.gainHP(amount)

class Monster:
    def __init__(self, hp=MONSTER_HP, player=None):
        self.hp = hp
        self.maxHP = hp
        self.points = 0
        self.player = player

    def getHP(self):
        return self.hp

    def gainHP(self, amount):
        healed = 0
        if self.hp + amount <= self.maxHP:
            healed = amount
        else:
            healed = self.maxHP - self.hp
        self.hp += healed
        return healed
    
    def loseHP(self, amount):
        loss = 0
        if self.hp - amount >= 0:
            loss = amount
        else:
            loss = self.hp
        self.hp -= loss
        return loss

    def doAction(self, response, amount, player, index):
        if response == 'attack':
            loss = player.loseHP(amount)
            monster_name = "Monster " + str(index)
            printAttackPlayer(monster_name, player.getName())
            printLostHP(player.getName(), loss, player.getHP())
        else:
            print("Error: doAction's response is invalid")

class Player:
    def __init__(self, hp=PLAYER_HP, name="Player 1"):
        self.hp = hp
        self.maxHP = hp
        self.points = 0
        self.name = name
        self.defend = False

    def getPoints(self):
        return self.points

    def gainPoints(self, amount):
        self.points += amount

    def getHP(self):
        return self.hp

    def isDefending(self):
        return self.defend

    def getName(self):
        return self.name

    def gainHP(self, amount):
        healed = 0
        if self.hp + amount <= PLAYER_HP:
            healed = amount
        else:
            healed = PLAYER_HP - self.hp
        self.hp += healed
        return healed
    
    def loseHP(self, amount):
        loss = 0
        if self.hp - amount >= 0:
            loss = amount
        else:
            loss = self.hp
        self.hp -= loss
        return loss

    def doAction(self, response, amount, monsters):
        if response == 'attack':
            index = printSelectMonster(monsters)
            loss = monsters[index].loseHP(amount)
            monster_name = "Monster " + str(index)
            printLostHP(monster_name, loss, monsters[index].getHP())
            self.defend = False
        elif response == 'defend':
            printDefend(self.name)
            self.defend = True
        elif response == 'heal':
            hp = self.gainHP(amount)
            printHeal(self.name, hp, self.getHP())
            self.defend = False
        else:
            print("Error: doAction's response is invalid")

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
        monsters = {}

        # For now, it's single player
        self.players.append(Player(name="Chicken Dinner"))

        # For each player
        for player in self.players:
            # Create an empty list
            monsters[player] = []

        printIntro(self.players)

        # While time allows, keep playing
        while timelimit > 0:
            # Players do actions on even turns
            if self.turn % 2 == 0:
                # For each player
                for player in self.players:
                    # If there are no monsters
                    if (not monsters[player]):
                        # Spawn one
                        monsters[player].append(Monster(hp=np.random.randint(1, 11)))

                    # Every 6 turns, spawn a monster
                    if self.turn != 0 and (self.turn % 6 == 0):
                        # Spawn one
                        monsters[player].append(Monster(hp=np.random.randint(1, 11)))

                    # If the player has no hp
                    if player.getHP() <= 0:
                        # Do nothing?
                        break
                    # Print the stats of player and monsters
                    printPlayerStats(player)

                    # Do a response
                    response = printPlayerChoice()
                    # For now, we'll say the amount is random
                    amount = np.random.randint(1, 6)

                    player.doAction(response, amount, monsters[player])
            # Monsters do actions on odd turns
            else:
                pass
                # For each player
                i = 0
                for player in self.players:
                    for monster in monsters[player]:
                        if monster.getHP() <= 0:
                            player.gainPoints(1000)
                            monsters[player].remove(monster)
                            print("Here!")
                            print(not monsters[player])

                        if not monsters[player]:
                            break
                    
                        # For now, the response is always attack
                        response = "attack"
                        # For now, we'll say the amount is random
                        amount = np.random.randint(1, 6)

                        monster.doAction(response, amount, player, i)
                        

                    printMonsterStats(monster, i)
                    i += 1
            self.turn += 1
            print("")
            # Subtract by time
            timelimit -= 0.5
        
        print("Game finished!")
        scores = np.zeros(len(self.players))
        i = 0
        for player in self.players:
            printScores(player)
            scores[i] = player.getPoints()
            i += 1

        index = np.argmax(scores)
        printWinner(self.players[index])

        

instance = Game()
instance.start()

# # !play
# @bot.command()
# async def play(ctx):
#     instance = Game()
#     instance.start(ctx)

# bot.run(TOKEN)