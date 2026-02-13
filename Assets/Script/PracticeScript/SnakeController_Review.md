# SnakeController.cs 코드 리뷰 및 학습 가이드

이 문서는 `SnakeController.cs`의 작동 원리와 사용된 핵심 프로그래밍 개념을 설명하여, 유니티와 C# 학습을 돕기 위해 작성되었습니다.

---

## 1. 클래스 개요
`SnakeController`는 이 게임의 주인공인 **뱀(Snake)**을 제어하는 핵심 스크립트입니다.
- **역할**: 사용자의 입력을 받아 뱀을 움직이고, 사과/골드를 먹으면 몸을 늘리며, 목적지에 도착하면 지나온 길을 '적의 이동 경로'로 변환(Bake)합니다.

---

## 2. 핵심 변수 설명

### 2.1. 싱글톤 패턴 (Singleton)
```csharp
public static SnakeController Instance { get; private set; }
```
- **설명**: 게임 내에 `SnakeController`는 단 하나만 존재해야 합니다. 다른 스크립트(예: `GameManager`, `SpawnManager`)에서 `SnakeController.Instance`라고만 쓰면 어디서든 이 스크립트에 접근할 수 있게 해주는 패턴입니다.
- **학습 포인트**: `static`은 "공유되는", "하나뿐인"이라는 뜻을 가집니다.

### 2.2. 입력 버퍼 (Queue)
```csharp
private Queue<Vector2Int> _inputQueue = new Queue<Vector2Int>();
```
- **질문**: 왜 그냥 `direction` 변수를 안 쓰고 `Queue`를 썼을까요?
- **이유**: 뱀이 한 칸 움직이는 동안 플레이어가 빠르게 '위 -> 오른쪽'을 연타했다고 가정해 봅시다. 변수 하나만 쓰면 마지막 입력인 '오른쪽'만 남아서, 위로 가려던 의도가 무시될 수 있습니다. `Queue`(줄 서기)를 사용하면 입력된 순서대로 차례차례 이동 명령을 수행하여 **조작감**을 향상시킵니다.

### 2.3. 몸통 리스트 (List)
```csharp
private List<Transform> _bodyParts = new List<Transform>();
```
- **설명**: 뱀의 머리부터 꼬리까지 모든 마디의 위치(`Transform`)를 저장합니다.
- **활용**: 이동할 때 꼬리가 앞 마디를 따라가게 하거나, 게임이 끝난 후 이 리스트를 적의 경로로 사용합니다.

---

## 3. 주요 함수 분석

### 3.1. 입력 처리 (`EnqueueDirection`)
```csharp
// 1. 반대 방향 전환 방지 (180도 턴 불가)
if (newDir == -lastPlannedDir) return;

// 2. 같은 방향 중복 입력 방지
if (newDir == lastPlannedDir) return;
```
- **논리**: 뱀 게임의 국룰입니다. 오른쪽으로 가다가 왼쪽 키를 누르면 자기 목을 조르게 되므로 막아야 합니다. 벡터의 덧셈 역원(`-Vector`)을 이용해 반대 방향인지 쉽게 체크합니다.

### 3.2. 이동 로직 (`MoveSnake`)
```csharp
// 꼬리부터 앞 마디의 위치로 한 칸씩 이동 (역순 루프)
for (int i = _bodyParts.Count - 1; i > 0; i--)
{
    _bodyParts[i].position = _bodyParts[i - 1].position;
}
// 머리 이동
transform.position += (Vector3Int)_currentDirection;
```
- **핵심**: **"뒤에서부터 앞으로"** 처리하는 것이 중요합니다!
    - 앞에서부터 처리하면, 2번 마디가 1번 마디 위치로 갔는데, 1번 마디는 이미 움직여버려서 3번 마디가 갈 곳을 잃어버릴 수 있습니다.
    - 그래서 꼬리(마지막)를 바로 앞 칸으로 옮기고, 그 앞 칸을 또 그 앞 칸으로 옮기는 식으로 처리해야 데이터 손실 없이 지렁이처럼 따라갑니다.

### 3.3. 경로 베이킹 (`BakePath`)
```csharp
for (int i = _bodyParts.Count - 1; i >= 0; i--)
{
    FinalPath.Add(_bodyParts[i].position);
}
```
- **개념**: 게임의 페이즈가 바뀔 때(Snake -> Defense), 뱀의 몸통 위치들이 곧 적이 걸어올 **길(Path)**이 됩니다.
- **로직**: 적은 꼬리 쪽에서 태어나 머리 쪽으로 와야 하므로, 리스트를 역순으로 순회하거나 저장된 리스트를 뒤집어서 사용합니다.

### 3.4. 충돌 처리 (`OnTriggerEnter2D`)
```csharp
if (collision.CompareTag("Apple")) { ... }
else if (collision.CompareTag("Gold")) { ... }
```
- **설명**: 뱀의 머리에 달린 콜라이더(Collider)가 다른 물체와 닿았을 때 발생합니다.
- **팁**: `CompareTag("Tag")`는 `tag == "Tag"`보다 성능이 좋고 안전합니다. (메모리 할당을 줄여줌)

---

## 4. 유용한 C# 문법 팁

### `StartCoroutine` & `IEnumerator`
- **사용처**: `MoveRoutine` 함수.
- **설명**: "0.2초 기다렸다가 움직여!" 같은 시간 제어가 필요할 때 씁니다. `Update` 함수에서 시간을 직접 재는 것보다 훨씬 읽기 쉽고 효율적입니다.
- `yield return new WaitForSeconds(0.2f);` -> "0.2초 동안 쉬고 다시 일해!"

### `TryGetComponent`
```csharp
if (part.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
```
- **설명**: "이 물체에 스프라이트 렌더러가 있니? 있으면 `sprite` 변수에 담아줘."
- **장점**: 컴포넌트가 없을 때 에러가 나는 것을 막아주고, `GetComponent`를 쓰고나서 `null` 체크하는 것보다 코드가 깔끔합니다.

---

## 5. 연습 과제 (Practice)
이 코드를 완전히 이해했는지 확인하기 위해 다음 기능들을 `PracticeController`에 직접 구현해보세요.

1.  **3단 가속**: 특정 키(Shift)를 누르고 있으면 이동 속도(`_moveInterval`)가 0.1초로 빨라지게 해보세요.
2.  **무적 모드**: 치트키(예: `I`)를 누르면 벽에 부딪혀도 죽지 않고 통과하게 만들어보세요. (Hint: `Collider2D.isTrigger`나 충돌 로직 수정)

작성자: Gemini CLI
