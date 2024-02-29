# 최종 프로젝트

## C++로 실시간 서버를 열어주고 Unity 게임에서 서버에 접속

서버는 최대 2명의 클라이언트를 수용하고 각 클라이언트가 접속할때마다 <br>
클라이언트의 접속 순서에따라 플레이어 number를 클라이언트에게 할당해주고 <br>
플레이어 number에 해당하는 플레이어 조작의 권한을 얻습니다. <br>
<br>

플레이어의 vector값과 flip 값은 실시간으로 서버에 전송되고 서버는 모든 클라이언트 들에게 <br>
위치 정보를 다시 전달하여 줍니다. <br>

+ 현재 수정해야할 사항은 너무 많은 값이 오고 가서 각각의 플레이어들이 부드럽게 움직이지 못하는 현상과 <br>
아직까지 공격이나 점프 모션의 애니메이션이 상대의 화면에서도 정상적으로 동작되지 않는것. 그리고 타이머의 시간을 아직 서버에 전달하는 것을 구현하지 못하여 추후에 수정해야할것 같습니다.


