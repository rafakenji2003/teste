# tests.py
from calculadora import somar, subtrair, multiplicar, dividir

def test_somar():
    assert somar(2, 3) == 5
    assert somar(-1, 1) == 0
    print("✓ somar OK")

def test_subtrair():
    assert subtrair(5, 3) == 2
    print("✓ subtrair OK")

def test_dividir_por_zero():
    try:
        dividir(1, 0)
        print("✗ deveria ter lançado erro")
    except ValueError:
        print("✓ divisão por zero OK")

if __name__ == "__main__":
    test_somar()
    test_subtrair()
    test_dividir_por_zero()
