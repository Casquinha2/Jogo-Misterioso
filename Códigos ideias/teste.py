
class Objetos:#+
    def __init__(self, nome):#+
        self.nome = nome#+
        self.show = False

    def setshow(self, show):#+
        self.show = show#+
    
    def getname(self):#+
        return self.nome

colecionaveis = []#+

total = 0#+
for i in range(1, 4):#+
    colecionaveis.append(Objetos(f"Objeto_{i}"))

colecionaveis_apanhados = []#+

colecionaveis_apanhados.append(Objetos("Objeto_1"))
colecionaveis_apanhados.append(Objetos("Objeto_2"))
colecionaveis_apanhados.append(Objetos("Objeto_3"))
inventario = []#+

for i in colecionaveis_apanhados:
    for j in colecionaveis:
        if i.getname() == j.getname():
            i.setshow(True)#+
            total += 1

if total == 3:
    print("Parabéns, apanhaste todos os colecionáveis!")#-~
else:
    print(f"""tens {len(colecionaveis_apanhados)} colecionaveis.""")
