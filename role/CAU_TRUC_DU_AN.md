# Cấu trúc dự án GameRTS

Cập nhật ngày 16/11/2026. Tài liệu giải thích các thư mục, file hiện có và cách chúng kết nối với nhau, giúp người đang học Unity tìm đúng nơi để đọc hoặc chỉnh sửa. Mọi đường dẫn trong tài liệu được tính từ thư mục gốc `GameRTS`.

Bản Claude nguyên văn nằm trong `TAI_LIEU_GOC_CLAUDE.txt`, chỉ đọc và đối chiếu. Kế hoạch thiết kế và tiến độ được cập nhật trong `Game RTS Unity Doc.md`. Luật làm việc nằm trong `role/LUAT_LAM_VIEC.md`; `AGENTS.md` ở gốc dẫn tới các tài liệu này.

## 1. Sơ đồ thư mục

```text
GameRTS/
├── AGENTS.md                       Điểm vào dẫn tới luật và tài liệu
├── Game RTS Unity Doc.md            Kế hoạch triển khai đang cập nhật
├── TAI_LIEU_GOC_CLAUDE.txt          Bản Claude nguyên văn, chỉ đọc
├── role/
│   ├── LUAT_LAM_VIEC.md             Luật bắt buộc mỗi lần làm việc
│   └── CAU_TRUC_DU_AN.md            Danh mục cấu trúc và chức năng file
├── .gitattributes                  Giữ nguyên byte của bản Claude qua Git
├── .gitignore                      Quy tắc bỏ qua file khi quản lý bằng Git
├── .vsconfig                       Cấu hình thành phần Visual Studio
├── Assets/
│   ├── Game/                       Mã nguồn và tài nguyên riêng của game
│   │   ├── Art/
│   │   │   ├── Terrain/            Sprite và Tile asset cho nền map
│   │   │   ├── Units/              Sprite quân và vòng chọn
│   │   │   ├── Resources/          Sprite cây, bụi quả và mỏ vàng mẫu
│   │   │   ├── Buildings/          Sprite TC, móng, Nhà, xưởng và ba nhà quân
│   │   │   └── UI/                 Icon tài nguyên, vòng tiến độ và cờ tập kết
│   │   ├── Commands/               Lệnh di chuyển, tấn công, khai thác, xây, huấn luyện và lệnh thử
│   │   ├── Data/
│   │   │   ├── Units/              Asset dữ liệu từng loại quân
│   │   │   ├── Resources/          Asset dữ liệu từng loại nguồn tài nguyên
│   │   │   └── Buildings/          Dữ liệu nơi nộp và loại công trình xây được
│   │   ├── Editor/                 Công cụ thiết lập scene, kiểm tra và build
│   │   ├── Input/                  Đọc chuột và bàn phím
│   │   ├── Scenes/                 Map kinh tế và map thử chiến đấu
│   │   ├── Simulation/             Lưới, A*, quân, ví từng người chơi và tick
│   │   ├── Tests/Editor/
│   │   │   ├── Simulation/         Kiểm thử A*, lệnh, va chạm và replay
│   │   │   └── Units/              Kiểm thử dữ liệu, Prefab và spawn
│   │   ├── Tests/PlayMode/          Kiểm thử chọn và điều khiển quân trong scene
│   │   ├── UI/                     HUD tài nguyên bằng uGUI, chỉ đọc Simulation
│   │   │   └── Prefabs/            EconomyHud: ví, trạng thái Dân, xây và huấn luyện
│   │   └── View/
│   │       └── Prefabs/            Khuôn GameObject dùng để tạo đối tượng
│   │           ├── Resources/      Prefab cây, bụi quả và mỏ vàng
│   │           └── Buildings/      TC, Nhà, xưởng và nhà quân có tiến độ xây
│   ├── Scenes/                     Scene có sẵn từ template Unity
│   └── Settings/                   Cấu hình và scene template Universal 2D
├── Packages/                       Danh sách package và phiên bản phụ thuộc
├── ProjectSettings/                Cấu hình chung của project Unity
├── Builds/                         Bản game đã build, Git bỏ qua
├── Library/                        Dữ liệu import và cache Unity, Git bỏ qua
├── Logs/                           Log và kết quả kiểm tra, Git bỏ qua
├── Temp/                           File tạm Unity, Git bỏ qua
└── UserSettings/                   Tùy chọn Editor theo máy, Git bỏ qua
```

`.git/` là dữ liệu quản lý phiên bản của repository này. Các file `.meta` nằm cạnh asset và thư mục trong `Assets`; quy tắc sử dụng được giải thích ở mục 9.

## 2. Tài liệu, luật và cấu hình

| File | Chức năng và cách sử dụng |
| --- | --- |
| `AGENTS.md` | Điểm vào ở gốc project, dẫn tới luật trong role, bản Claude chỉ đọc, kế hoạch Markdown và danh mục cấu trúc. |
| `role/CAU_TRUC_DU_AN.md` | Danh mục các file và thư mục, chức năng, quan hệ và cách tìm phần cần chỉnh. Cập nhật mỗi khi thêm file được giữ lại hoặc thay đổi cấu trúc. |
| `Game RTS Unity Doc.md` | Kế hoạch triển khai đang cập nhật, chuyển đủ 187 đoạn và giữ 4 bảng thiết kế từ bản DOCX. Đầu file có bảng tổng quan tiến độ Mốc 0–8 và bằng chứng kiểm chứng mới nhất; mục 14 ghi chi tiết/lịch sử từng bước, mục 16 có checklist đã làm/chưa làm. Phân biệt phần triển khai, kiểm thử tự động và nghiệm thu PC thủ công. |
| `TAI_LIEU_GOC_CLAUDE.txt` | Nguyên văn nội dung Claude do người dùng cung cấp, chỉ đọc/đối chiếu, không chỉnh sửa hoặc xóa. Đặt Read-only trên Windows; SHA-256 được lưu trong file luật. |
| `role/LUAT_LAM_VIEC.md` | Luật đọc tài liệu trước khi làm, bảo vệ bản Claude, giải thích chi tiết khi hoàn thành và cập nhật danh mục file. |
| `.gitattributes` | Ngăn Git tự đổi kiểu xuống dòng của bản Claude để nội dung byte và SHA-256 ổn định qua commit/checkout. |
| `.gitignore` | Quy định file và thư mục Git bỏ qua, gồm cache, log, bản build, file IDE và tùy chọn theo máy. |
| `.vsconfig` | Ghi thành phần Visual Studio phục vụ phát triển game bằng Unity; IDE có thể dùng file này để đề xuất thành phần cần cài. |

## 3. Mã nguồn gameplay và dữ liệu

Assembly Definition là file `.asmdef` chia mã C# thành các nhóm biên dịch và quy định nhóm nào được tham chiếu nhóm nào. Tham chiếu tới một assembly cho phép dùng mã trong đó; riêng việc có tham chiếu không có nghĩa tính năng gameplay đã được triển khai.

### Simulation và Commands

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Simulation/GameRTS.Simulation.asmdef` | Định nghĩa assembly mô phỏng C# tham chiếu Commands; `noEngineReferences` bảo đảm không tham chiếu UnityEngine. |
| `Assets/Game/Simulation/GridMap.cs` | Lưới 8 hướng chi phí 1000/1414, kiểm tra góc chéo; 3 lớp địa hình, resourceBlocked và buildingBlocked cùng quyết định IsWalkable. CellAt từ chối tọa độ ngoài map; cạn nguồn không gỡ nhầm lớp địa hình/công trình. |
| `Assets/Game/Simulation/GridPathfinder.cs` | A* với heap, 8 hướng, chi phí cạnh/chéo 1000/1414 và ước lượng octile theo khoảng cách ngang, dọc và chéo. Dùng CanTraverse để tránh cắt góc tường và các ô bị giữ chỗ; thứ tự phân xử thống nhất giữa các máy. |
| `Assets/Game/Simulation/UnitState.cs` | Trạng thái quân, di chuyển/SegmentProgress, CombatStats, HP, mục tiêu và hồi chiêu. Khi chết giữ bản ghi ID, giải phóng giữ chỗ và ẩn View; lệnh cũ không trỏ vào quân mới. Dân có thêm VillagerState khi carryCapacity > 0; lính có Villager = null. Chứa trạng thái thật bằng C# thuần, View chỉ đọc. |
| `Assets/Game/Simulation/SimulationWorld.cs` | Mô phỏng 20 tick/giây, ví/quân/nguồn và hàng đợi Command chung. Điều phối kinh tế/xây/huấn luyện/chiến đấu; cuối tick cập nhật khám phá và cho AI ra quyết định nếu tới nhịp. Checksum gồm trạng thái gameplay, tầm nhìn/khám phá và AI. AI không tự sửa ví hoặc sinh quân ngoài Command. |
| `Assets/Game/Simulation/SimulationWorld.Economy.cs` | Gather, chia ô làm, chọn nơi nộp đúng chủ/loại theo đường đi; giữ ba lô, nộp chuyến cuối chưa đầy. Nguồn cạn tìm cùng loại trong 8 ô, phân xử đường/ID/ô và retry khi bị chặn. Khi bật tầm nhìn, Gather cần nguồn đã khám phá; tự đổi nguồn chỉ xét nguồn đang thấy để không biết mỏ bí mật. |
| `Assets/Game/Simulation/BuildingState.cs` | PlacementIssue, BuildingDefinition bất biến (ID, footprint, chi phí, lượng công, HP, sức chứa, loại nhận hàng) và BuildingState giữ chủ sở hữu/ô gốc/CompletedWork/HitPoints/ProgressPermille. Có Armor/IsDestroyed và TrainingDefinition tùy chọn; ID lần đầu = ô gốc + 1, xây lại dùng ID mới trên Map.Count. Chỉ hoàn thành mới kích hoạt chức năng; bản ghi đã phá giữ để từ chối ID cũ. |
| `Assets/Game/Simulation/SimulationWorld.Buildings.cs` | Catalog/kiểm tra footprint và đường Dân; Build trừ ví một lần, móng chặn lưới, ResumeBuild miễn phí. Giữ ResourceId khi giao xây; EndBuildingJob tự nộp ba lô rồi quay lại thu. Tăng tiến độ/HP theo công mới, không ghi đè sát thương đã nhận; hoàn thiện tăng sức chứa/kích hoạt nơi nộp đúng một lần. Bỏ qua công trình đã phá; nhà quân hoàn thiện mới đăng ký TrainingState, không nhận nộp tài nguyên. |
| `Assets/Game/Simulation/VillagerState.cs` | Trạng thái Dân: công việc, nguồn/nơi nộp/công trình, ba lô, WorkTicks/RetryTicks. Có nhóm xây và SearchingForResource khi chờ đường tới nguồn thay thế. ResourceId được nhớ qua việc xây; Move xóa nguồn. Các trạng thái đều tham gia checksum. |
| `Assets/Game/Simulation/DropOffState.cs` | Dữ liệu nơi nộp và HP/giáp: ID theo ô gốc + 1, chủ sở hữu, ô gốc/kích thước và mặt nạ ResourceAcceptance Wood/Food/Gold/All. TC có MaxHP/CurrentHP/Armor; xưởng hoàn thiện liên kết HP với BuildingState. Nơi nộp bị phá rời danh sách hoạt động, bản ghi vẫn giữ cho ID/checksum. Công trình chặn lưới; Dân tương tác ở ô sát cạnh footprint. |
| `Assets/Game/Simulation/PlayerEconomyState.cs` | Ví số nguyên gồm Wood/Food/Gold/Population/MaxPopulation. Nạp, thanh toán nhiều loại cùng lúc, giữ/giải phóng dân số và tăng/giảm sức chứa; từ chối số âm, thiếu tiền, spawn vượt sức chứa hoặc tràn int mà không thay đổi dở dang. Nhà bị phá có thể làm dân hiện có vượt trần mới; không xóa dân, chặn sinh thêm tới khi có chỗ. |
| `Assets/Game/Simulation/ResourceNodeState.cs` | Enum ResourceType (Wood/Food/Gold) và trạng thái C# thuần: ID, ID loại, ô chiếm, trữ lượng ban đầu/còn lại, IsDepleted. Mỗi nguồn có trạng thái riêng, không sửa ResourceDataSO khi khai thác; nguồn cạn vẫn giữ ID để lệnh cũ không tác động nhầm nguồn khác. |
| `Assets/Game/Simulation/DeterminismScenario.cs` | Replay phiên bản 12 ghi 600 hàng, mỗi hàng 10 checksum: chín thế giới trước và một thế giới AI. Chín thế giới cũ bước 1 tick/hàng; AI bước 10 tick/hàng (tối đa 6000 tick, dừng khi kết thúc ván) để bao trùm kinh tế/quân sự/tấn công. Không đưa FPS/ghost vào kết quả. |
| `Assets/Game/Simulation/VillagerScenario.cs` | CreateWorld: 3 Dân thu đủ 25 mỗi loại. CreateRecoveryWorld: Dân đang mang 10 bị giao xây, sau đó tự nộp/thu phần cuối 2, tìm cây kế tiếp 8; tổng 20 Gỗ bảo toàn và chi 50 Gỗ xây Nhà. Dùng chung trong test/replay Editor và Windows. |
| `Assets/Game/Simulation/ConstructionScenario.cs` | CreateWorld kiểm tra nhà/kho, ngắt/tiếp tục. CreateCampWorld thêm 3 Dân xây xưởng 2 × 2, mỗi xưởng 100 Gỗ rồi thu/nộp đủ 25 mỗi loại; TC All vẫn đặt xa. Cấu hình công rút ngắn trong kịch bản test, không thay số liệu asset gameplay. Dùng chung Editor/Windows. |
| `Assets/Game/Commands/GameRTS.Commands.asmdef` | Định nghĩa assembly dành cho dữ liệu lệnh, không tham chiếu UnityEngine. |
| `Assets/Game/Commands/MoveCommand.cs` | Lệnh di chuyển bất biến chứa tick thực thi, mã người chơi, số thứ tự, danh sách ID đã sắp xếp và tọa độ đích số nguyên. Input tạo lệnh, SimulationWorld kiểm tra và thực thi. |
| `Assets/Game/Commands/GatherCommand.cs` | Lệnh bất biến Tick/PlayerId/Sequence, danh sách ID được sao chép/sắp xếp/loại trùng và NodeId. World xác thực toàn bộ quyền điều khiển/khả năng thu trước khi áp dụng. Không cần bật cờ debug để khai thác thật. |
| `Assets/Game/Commands/BuildCommand.cs` | Lệnh đặt móng bất biến: Tick/PlayerId/Sequence, TypeId, ô gốc và danh sách BuilderIds được sao chép/sắp xếp/loại trùng. Giá và kích thước lấy từ catalog tin cậy, không nhận theo ý lệnh gửi vào. |
| `Assets/Game/Commands/ResumeBuildCommand.cs` | Giao Dân cho công trình đang xây theo BuildingId, cùng tick/owner/sequence và BuilderIds bất biến. World kiểm tra chủ sở hữu và trạng thái móng; không thu thêm chi phí. |
| `Assets/Game/Commands/ISimulationCommand.cs` | Hợp đồng C# thuần Tick/PlayerId/Sequence dùng chung cho các lệnh; SimulationWorld dùng chung hàng đợi và bộ lọc lệnh trùng. |
| `Assets/Game/Commands/EconomyDebugCommand.cs` | Lệnh bất biến để thử nạp, chi 3 loại tài nguyên hoặc tăng giới hạn dân số. Chứa enum thao tác và lượng nguyên dương; bị từ chối mặc định khi SimulationWorld không bật cho phép thử ví. |
| `Assets/Game/Commands/ResourceDebugCommand.cs` | Lệnh thử rút trữ lượng chứa Tick/PlayerId/Sequence/NodeId/Amount. Chạy theo tick và sequence chung; chỉ nhận khi World bật AllowResourceDebugCommands. Không cộng tiền vào ví, chưa thay cho lệnh dân thu hoạch. |

Simulation giữ trạng thái di chuyển và thực thi MoveCommand bằng phép tính số nguyên theo tick. Một đơn vị thế giới bằng 1000 đơn vị mô phỏng. Lưới dùng ô 1 × 1 và bán kính quân nhỏ hơn 0,5 đơn vị thế giới. Quân đứng chiếm một ô; khi di chuyển giữ ô hiện tại, ô tiếp theo và thêm hai ô sát góc nếu đi chéo để ngăn hai đường chéo giao nhau. Lớp kết nối và truyền lệnh lockstep qua mạng vẫn thuộc mốc multiplayer.

### Data

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Data/GameRTS.Data.asmdef` | Assembly cấu hình ScriptableObject dùng UnityEngine; tham chiếu Simulation để dùng ResourceType C# thuần, không tạo phụ thuộc ngược từ Simulation sang Unity/Data. |
| `Assets/Game/Data/UnitDataSO.cs` | ID, tên, tốc độ, bán kính, Prefab và nhóm Villager: Can Gather, Carry Capacity, Gather Interval Ticks. Có UnitClass/MaxHP/Armor/AttackDamage/AttackIntervalTicks/AttackRangeCells/ProjectileSpeed/AggroRangeCells. CreateCombatStats chuyển sang dữ liệu C# thuần; CarryCapacity đưa vào Simulation là 0 nếu tắt Can Gather; chuẩn hóa cấu hình trong OnValidate. |
| `Assets/Game/Data/Units/SampleSoldierData.asset` | Dữ liệu loại `sample_soldier`, tên Lính mẫu, tốc độ 3, bán kính 0,45; tham chiếu SampleSoldier.prefab phục vụ xem Prefab/kiểm thử độc lập; GameScene khởi đầu chỉ dùng VillagerData. Các chỉ số là giá trị thử nghiệm. |
| `Assets/Game/Data/Units/VillagerData.asset` | Dữ liệu Dân: ID villager, tốc độ 3, bán kính 0,45, Can Gather bật, tải 10, chu kỳ 10 tick và Villager.prefab. |
| `Assets/Game/Data/DropOffDataSO.cs` | ScriptableObject nơi nộp: tên, Width/Height 1..8 ô, ResourceAcceptance và Sprite. Menu Create > GameRTS > Drop Off Data; bộ lọc cho phép cấu hình xưởng theo từng loại. TC có thêm MaxHP/Armor để làm mục tiêu chiến đấu; dữ liệu xây/huấn luyện nằm ở SO riêng. |
| `Assets/Game/Data/Buildings/TownHallData.asset` | Tòa thị chính mẫu footprint 2 × 2, nhận cả 3 tài nguyên, TownHall.png; dùng bởi TownHall.prefab. |
| `Assets/Game/Data/BuildingDataSO.cs` | Dữ liệu xây: ID/tên, Width/Height, 3 chi phí, RequiredWork, MaxHitPoints, PopulationBonus, ResourceAcceptance, sprite móng/hoàn thiện và Prefab. Có Armor và TrainingDataSO tùy chọn cho nhà quân; CreateDefinition chuyển sang dữ liệu C# thuần trước tick 1. |
| `Assets/Game/Data/Buildings/HouseData.asset` | Nhà mẫu ID house, 2 × 2, 50 Gỗ, 200 công (10 giây một Dân đã tới chỗ), 300 HP, +5 sức chứa khi hoàn thiện; không nhận hàng. Tham chiếu hai sprite và House.prefab. |
| `Assets/Game/Data/Buildings/LumberCampData.asset` | Xưởng Gỗ, ID lumber_camp: 2 × 2, 100 Gỗ, 200 công, 400 HP, không tăng dân số, chỉ nhận Wood sau khi xây xong. Dùng LumberCamp.prefab và sprite riêng. |
| `Assets/Game/Data/Buildings/MillData.asset` | Xưởng Thực, ID mill: 2 × 2, 100 Gỗ, 200 công, 400 HP; chỉ nhận Food, không tăng dân số. Dùng Mill.prefab và sprite riêng. |
| `Assets/Game/Data/Buildings/MiningCampData.asset` | Xưởng Vàng, ID mining_camp: 2 × 2, 100 Gỗ, 200 công, 400 HP; chỉ nhận Gold, không tăng dân số. Dùng MiningCamp.prefab và sprite riêng. |
| `Assets/Game/Data/ResourceDataSO.cs` | ScriptableObject định nghĩa ID loại, tên, ResourceType, InitialAmount và Sprite. Menu Create > GameRTS > Resource Data; trữ lượng dương, tên/ID được trim khi chỉnh Inspector. |
| `Assets/Game/Data/Resources/TreeData.asset` | Dữ liệu cây: Wood, 100, Tree.png; dùng bởi Tree.prefab và các bản sao cây trong map. |
| `Assets/Game/Data/Resources/BerryBushData.asset` | Dữ liệu bụi quả: Food, 150, BerryBush.png; dùng bởi BerryBush.prefab. |
| `Assets/Game/Data/Resources/GoldMineData.asset` | Dữ liệu mỏ vàng: Gold, 800, GoldMine.png; dùng bởi GoldMine.prefab. |

ScriptableObject là asset chứa cấu hình dùng chung cho một loại quân. Trạng thái riêng của từng quân nằm trong UnitState của Simulation. Tốc độ và bán kính lưu dạng số thực trong cấu hình; RtsGameView chuyển sang số nguyên theo tỷ lệ 1000 trước khi tạo trạng thái mô phỏng.

### Input

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Input/GameRTS.Input.asmdef` | Assembly điều khiển tham chiếu Commands, Simulation, Data, View, UI và Input System; UI không tham chiếu ngược Input. |
| `Assets/Game/Input/RtsCameraInput.cs` | Gắn Main Camera, đọc WASD, mũi tên, con lăn, edge scrolling rồi gọi RtsCameraView.ApplyInput. Kiểm tra focus/Game View; không zoom hoặc cuộn mép khi con trỏ nằm trên top bar hoặc bảng thử ví. |
| `Assets/Game/Input/UnitSelectionInput.cs` | Click/kéo/Shift chọn quân còn sống thuộc player1; tự bỏ ID chết. Chuột phải quân/nhà địch ưu tiên tạo AttackCommand có loại mục tiêu Unit/Building. Chuột phải nguồn tạo GatherCommand, chuột phải móng nhà mình chưa xong tạo ResumeBuildCommand cho Dân; quân khác giữ lệnh cũ. Khi chọn TC, chuột phải gửi TrainingCommand.SetRally; khi chọn quân, chuột phải nền tạo MoveCommand và ngắt công việc. Dùng chung sequence, bỏ qua HUD/gesture đặt móng và kiểm tra focus. |
| `Assets/Game/Input/EconomyTestInput.cs` | F9 mở/đóng bảng thử ví; nhận nút bấm từ HUD, tạo EconomyDebugCommand sau 2 tick với lượng 50 mỗi loại hoặc tăng sức chứa 5. Dùng NextLocalSequence chung với di chuyển; không trực tiếp sửa ví. |
| `Assets/Game/Input/ResourceTestInput.cs` | Trỏ nguồn + F8 hoặc click chuột giữa rút thử 50; giữ Shift để rút hết. Bỏ qua HUD/ngoài viewport/nguồn cạn/chế độ đặt móng; tạo ResourceDebugCommand sau 2 tick với sequence chung. Bật/tắt theo Enable Resource Testing. |
| `Assets/Game/Input/BuildingPlacementInput.cs` | Nút xây gửi TypeId để bật/chuyển ghost Nhà hoặc ba xưởng; B vẫn chọn Nhà. Trái gửi BuildCommand, phải/Esc hủy. API BeginPlacement nhận ID loại, dùng sequence chung; chỉ Dân được giao xây. Chuột phải móng giao ResumeBuild qua UnitSelectionInput. |

### View và Prefab

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/View/GameRTS.View.asmdef` | Định nghĩa assembly hiển thị dùng UnityEngine, tham chiếu Simulation, Commands và Data. |
| `Assets/Game/View/RtsCameraView.cs` | Gắn trên Main Camera, pan theo thời gian, zoom Orthographic và clamp toàn bộ vùng nhìn vào biên Tilemap Ground. Nhận điều khiển từ RtsCameraInput. |
| `Assets/Game/View/UnitView.cs` | Liên kết dữ liệu/ID, vòng chọn và nội suy vị trí. Có thanh máu, nhãn HP và chớp đòn đánh theo tick; chết ẩn cả GameObject/vòng chọn. Dân có Cargo/marker hàng và búa quay khi đứng xây. Chỉ đọc trạng thái; thời gian hiệu ứng không quyết định thu, nộp hay tiến độ xây. |
| `Assets/Game/View/UnitPreviewSpawner.cs` | Spawn Positions và Unit Data mặc định, thêm Unit Data Overrides theo từng vị trí. GameScene cấu hình 3 vị trí đều là VillagerData; lớp vẫn giữ mặc định 5 để xem thử độc lập. Giữ chế độ xem Prefab độc lập khi không gán game. |
| `Assets/Game/View/RtsMapView.cs` | Gắn trên Map Grid, chuyển ô Ground có nền và không có tile ở Obstacles thành ô đi được trong GridMap lúc khởi tạo. Kiểm tra map dùng ô XY 1 × 1. |
| `Assets/Game/View/RtsGameView.cs` | Khởi tạo World/catalog/nguồn/TC và dữ liệu Prefab; tự tạo View khi Simulation sinh quân/nhà, nội suy và pool đạn. Khi World bật tầm nhìn, ẩn quân/nhà địch, nguồn và đạn ngoài vùng thấy của player1; không dùng View để quyết định AI. GameScene/CombatScene giữ chế độ cũ khi không bật tầm nhìn. |
| `Assets/Game/View/BuildingView.cs` | Gắn House.prefab, đặt hình theo footprint và đọc BuildingState để đổi sprite móng/hoàn thiện, thanh tiến độ, nhãn %. Tắt thanh xây khi xong, có thanh HP riêng và ẩn khi phá; không thay đổi công hoặc tài nguyên. |
| `Assets/Game/View/BuildingPlacementView.cs` | Ghost sprite cùng từng ô footprint xanh/đỏ, chỉ đọc CheckBuildingPlacement. Giữ chế độ đặt và chặn gesture chọn/di chuyển xuyên qua lần xác nhận hoặc hủy. Đây là trạng thái View cục bộ, không thuộc checksum. |
| `Assets/Game/View/DropOffView.cs` | Khởi tạo nơi nộp cùng HP/giáp từ DropOffDataSO và ownerId; chuyển vị trí tâm hình thành ô gốc footprint cho World.AddDropOff. TC có TrainingDataSO đăng ký hàng đợi trước tick 1; Render bật vòng chọn/cờ và thanh HP, ẩn TC khi bị phá; không quyết định sinh quân. |
| `Assets/Game/View/ResourceNodeView.cs` | ResourceDataSO + SpriteRenderer + TextMesh Stock. Initialize đăng ký nguồn vào World và đặt ở tâm ô; Render đọc Remaining để đổi nhãn, tắt sprite/nhãn khi cạn. Gizmo vàng hiển thị footprint 1 ô trong Scene View. Không dùng Collider2D/Unity Physics để chặn quân. |
| `Assets/Game/View/UnitSelectionView.cs` | Giữ danh sách ID được chọn tại máy người chơi. Xử lý click, kéo khung mọi hướng, Shift chọn thêm/bỏ chọn và vẽ khung quét trên màn hình. Cập nhật vòng chọn qua UnitView. Click footprint TC/nhà quân hoàn thiện chọn độc quyền SelectedTrainingBuildingId; chọn/quét quân bỏ chọn TC. |
| `Assets/Game/View/ReplayVerification.cs` | Nhận -rtsReplayCheck, ghi replay12 gồm 600 hàng×10 checksum, có thêm chu trình AI phát triển/huấn luyện/tấn công, rồi thoát mã0 hoặc1. Không tác động luồng chơi bình thường. |
| `Assets/Game/View/Prefabs/SampleSoldier.prefab` | Khuôn quân mẫu gồm SpriteRenderer và UnitView ở gốc, con SelectionCircle có SpriteRenderer và mặc định tắt. Tham chiếu hai sprite trong Art/Units; UnitDataSO tham chiếu Prefab này. |
| `Assets/Game/View/Prefabs/Villager.prefab` | Prefab Dân có sprite riêng, vòng chọn, Cargo 0/10, Cargo Marker và Hammer mặc định ẩn, gán vào UnitView. Dữ liệu nằm trong VillagerData.asset. |
| `Assets/Game/View/Prefabs/Buildings/TownHall.prefab` | Prefab Tòa thị chính mẫu có DropOffView, SpriteRenderer và nhãn; chiếm 2 × 2 ô, owner1 trong scene, nhận All; gắn VillagerTrainingData, Selection Marker và Rally Flag. |
| `Assets/Game/View/Prefabs/Buildings/House.prefab` | BuildingView với Body, Progress Background/Fill và nhãn tiến độ. Hình tự co theo footprint, hoàn thiện đổi sprite và tiếp tục giữ ô cản. |
| `Assets/Game/View/Prefabs/Buildings/LumberCamp.prefab` | BuildingView của Xưởng Gỗ với móng dùng chung, thanh tiến độ và nhãn; khi xong dùng LumberCamp.png theo LumberCampData. |
| `Assets/Game/View/Prefabs/Buildings/Mill.prefab` | BuildingView của Xưởng Thực, giữ tham chiếu Body/tiến độ/HP/nhãn; hoàn thiện đổi sang Mill.png. |
| `Assets/Game/View/Prefabs/Buildings/MiningCamp.prefab` | BuildingView của Xưởng Vàng, móng và tiến độ dùng hệ xây chung, hoàn thiện đổi sang MiningCamp.png. |
| `Assets/Game/View/Prefabs/Resources/Tree.prefab` | Khuôn cây, SpriteRenderer với Tree.png, ResourceNodeView dùng TreeData và nhãn trữ lượng 100. |
| `Assets/Game/View/Prefabs/Resources/BerryBush.prefab` | Khuôn bụi quả, BerryBush.png/BerryBushData và nhãn 150. |
| `Assets/Game/View/Prefabs/Resources/GoldMine.prefab` | Khuôn mỏ vàng, GoldMine.png/GoldMineData và nhãn 800. |

`UnitId` trong dữ liệu xác định loại quân; `InstanceId` của UnitView trong GameScene nhận từ ID do SimulationWorld cấp. Chọn quân và góc nhìn là trạng thái cục bộ. Vị trí thật và quyết định di chuyển do Simulation quản lý.

### UI — Thanh tài nguyên

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/UI/GameRTS.UI.asmdef` | Assembly giao diện tham chiếu View, Simulation, Commands, Data và UnityEngine.UI. |
| `Assets/Game/UI/EconomyHudView.cs` | Đọc ví/top bar và Worker Status gồm việc/ba lô/trữ lượng/tiến độ, lấy đúng tên Nhà hoặc xưởng từ catalog. Hiện chờ đường tới nguồn thay thế hoặc hết nguồn cùng loại trong 8 ô. Các panel/nút xây và panel Training chặn click xuyên xuống map. |
| `Assets/Game/UI/Prefabs/EconomyHud.prefab` | Canvas 1280 × 720: ví, thử ví, Worker Status (có HP/công việc đánh) và Construction Notice. Có nút Switch Map góc phải dưới giữa GameScene và CombatScene (mỗi lần bắt đầu ván mới). Bốn nút Nhà/Gỗ/Thực/Vàng nằm dưới top bar, hiện khi chọn Dân, có giá trên nút; thông báo đặt móng chuyển xuống dưới hàng nút. Training Panel góc trái dưới hiện khi chọn TC: nút Dân, vòng tiến độ, 5 icon và thông báo. |
| `Assets/Game/UI/ConstructionHudView.cs` | Bảy nút xây (Nhà/ba xưởng/ba nhà quân) phát TypeId qua BuildRequested; RefreshBuildOptions chỉ hiện khi có Dân được chọn. Đọc giá/kích thước, báo lý do/kết quả đặt móng. Bounds của từng nút và Construction Notice đều chặn pointer xuống map. |

HUD chỉ đọc ví. UnitSelectionInput bỏ qua click/chuột phải trên top bar hoặc bảng thử; RtsCameraInput bỏ qua edge scrolling và zoom khi con trỏ ở các vùng đó. WASD vẫn dùng được. EventSystem trong GameScene dùng InputSystemUIInputModule để các Button nhận được Input System mới.

## 4. Scene và hình ảnh riêng của game

| File | Chức năng và nơi sử dụng |
| --- | --- |
| `Assets/Game/Scenes/GameScene.unity` | Scene build: 3 Dân, 9 nguồn, ví/HUD, TC All đặt sẵn tâm (2,-5), 2 × 2. RTS Session có catalog Nhà + ba xưởng + ba nhà quân, placement/input và Constructed Buildings chứa công trình được xây. Không đặt sẵn xưởng; người chơi chọn Dân để xây. TrainingInput trên RTS Session nối sự kiện UI vào Command chung. |
| `Assets/Game/Art/Terrain/GroundTile.png` | Sprite nền 32 × 32 pixel, Pixels Per Unit 32, dùng chung cho hai Tile asset nền cỏ. |
| `Assets/Game/Art/Terrain/GrassLight.asset` | Tile nền màu cỏ sáng, tham chiếu GroundTile.png và được vẽ lên Ground trong GameScene. |
| `Assets/Game/Art/Terrain/GrassDark.asset` | Tile nền màu cỏ tối, cùng sprite nhưng màu khác, xếp xen kẽ với GrassLight để dễ quan sát map. |
| `Assets/Game/Art/Terrain/RockObstacle.asset` | Tile màu nâu cho lớp Obstacles, dùng GroundTile.png. Tô tile này lên Obstacles bằng Tile Palette để đặt ô cấm đi trước khi Play. |
| `Assets/Game/Art/Units/SampleSoldier.png` | Sprite quân mẫu nhìn từ trên xuống, 64 × 64 pixel, nền trong suốt, Pixels Per Unit 64; dùng bởi SpriteRenderer ở gốc Prefab quân. |
| `Assets/Game/Art/Units/SelectionCircle.png` | Sprite vòng xanh 64 × 64 pixel, nền trong suốt, Pixels Per Unit 64; dùng bởi đối tượng con SelectionCircle. |
| `Assets/Game/Art/Units/Villager.png` | Sprite Dân mẫu 64 × 64 RGBA, PPU64/Point, áo nâu/mũ sáng và công cụ; phân biệt với lính xanh. |
| `Assets/Game/Art/Buildings/TownHall.png` | Sprite Tòa thị chính mẫu 128 × 128 RGBA, PPU64/Point, chiếm hình 2 × 2 đơn vị thế giới. |
| `Assets/Game/Art/Buildings/HouseFoundation.png` | Sprite móng Nhà mẫu 128 × 128 RGBA/PPU64/Point, dùng lúc tiến độ chưa đủ và trên House.prefab. |
| `Assets/Game/Art/Buildings/House.png` | Sprite Nhà mái xanh hoàn thiện, 128 × 128 RGBA/PPU64/Point; cũng dùng làm bóng preview. |
| `Assets/Game/Art/Buildings/LumberCamp.png` | Sprite xưởng mái xanh lá, có chồng gỗ, 128 × 128 RGBA/PPU64/Point, dùng cho preview và công trình hoàn thiện. |
| `Assets/Game/Art/Buildings/Mill.png` | Sprite xưởng mái đỏ, bao lương thực màu vàng, 128 × 128 RGBA/PPU64/Point. |
| `Assets/Game/Art/Buildings/MiningCamp.png` | Sprite xưởng mái xám xanh với đá/vàng, 128 × 128 RGBA/PPU64/Point. |
| `Assets/Game/Art/Units/Hammer.png` | Sprite búa 16 × 16 RGBA/PPU16/Point, con Hammer trong Villager.prefab. UnitView chỉ bật/quay búa khi Dân đứng xây; chuyển động không ảnh hưởng tiến độ. |
| `Assets/Game/Art/Resources/Tree.png` | Sprite cây mẫu tự tạo bằng hình pixel, 64 × 64 RGBA, PPU64, lọc Point; hình nằm trong footprint 1 ô, dùng bởi TreeData/Tree.prefab. |
| `Assets/Game/Art/Resources/BerryBush.png` | Sprite bụi xanh có quả đỏ mẫu, 64 × 64 RGBA, PPU64, lọc Point. |
| `Assets/Game/Art/Resources/GoldMine.png` | Sprite đá xám có vệt vàng mẫu, 64 × 64 RGBA, PPU64, lọc Point. |
| `Assets/Game/Art/UI/FoodIcon.png` | Icon quả đỏ 64 × 64, nền trong suốt, hình mẫu tự tạo bằng công cụ Editor, dùng ô Thực của HUD. |
| `Assets/Game/Art/UI/WoodIcon.png` | Icon hai khúc gỗ 64 × 64, nền trong suốt, dùng ô Gỗ. |
| `Assets/Game/Art/UI/GoldIcon.png` | Icon thỏi vàng 64 × 64, nền trong suốt, dùng ô Vàng. |
| `Assets/Game/Art/UI/PopulationIcon.png` | Icon nhóm người 64 × 64, nền trong suốt, dùng ô Dân số hiện tại/giới hạn. |

Ground là Tilemap 64 × 64 ô, mỗi ô 1 × 1, biên thế giới từ -32 đến 32 trên mỗi trục. Obstacles là Tilemap riêng có cùng hệ tọa độ; RtsMapView đọc hai lớp để tạo lưới di chuyển lúc khởi tạo. Thay đổi Tilemap trong lúc đang chơi chưa tự cập nhật lưới mô phỏng.

## 5. Kiểm thử

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Tests/Editor/GameRTS.Camera.Tests.asmdef` | Assembly kiểm thử chỉ dùng trong Editor, tham chiếu View, Input và Unity.InputSystem, sử dụng Unity Test Framework. |
| `Assets/Game/Tests/Editor/RtsCameraTests.cs` | Kiểm thử phím điều khiển, pan, hướng chéo, giới hạn zoom, clamp viewport và trường hợp kích thước màn hình/map khác nhau. |
| `Assets/Game/Tests/Editor/Units/GameRTS.Units.Tests.asmdef` | Assembly kiểm thử dữ liệu quân/nguồn/công trình trong Editor; tham chiếu Data, View, Simulation, UI và Unity.ugui để kiểm tra nút xây/Prefab. |
| `Assets/Game/Tests/Editor/Units/UnitPreviewTests.cs` | Kiểm thử Prefab, bật/tắt và khởi tạo lại vòng chọn, spawn 5 bản sao có ID riêng, dữ liệu dùng chung, đầu vào không hợp lệ và chuẩn hóa cấu hình. |
| `Assets/Game/Tests/Editor/Units/ResourceDataTests.cs` | Kiểm tra 3 asset/Prefab: loại, trữ lượng, sprite, dữ liệu tham chiếu, nhãn và không có Collider2D. Assembly Units.Tests thêm tham chiếu Simulation để dùng ResourceType. |
| `Assets/Game/Tests/Editor/Units/VillagerAssetTests.cs` | Kiểm tra cấu hình Dân/tải/nhịp, Prefab ba lô và dữ liệu Tòa thị chính nhận All với footprint 2 × 2. |
| `Assets/Game/Tests/Editor/Units/BuildingAssetTests.cs` | Kiểm tra dữ liệu Nhà, chi phí/footprint/công, hai giai đoạn sprite, thanh tiến độ, nút xây và búa Dân được gán. |
| `Assets/Game/Tests/Editor/Units/DropOffCampAssetTests.cs` | Kiểm tra ba bộ dữ liệu/Prefab/sprite xưởng, giá và bộ lọc đúng loại, 200 công/400 HP, không tăng sức chứa; bốn nút xây được gán và mặc định ẩn. |
| `Assets/Game/Tests/Editor/Simulation/GameRTS.Simulation.Tests.asmdef` | Assembly kiểm thử Editor tham chiếu Simulation và Commands để kiểm tra lõi C# mà không cần GameObject gameplay. |
| `Assets/Game/Tests/Editor/Simulation/SimulationTests.cs` | Kiểm thử A*, tốc độ ở cả 8 hướng, cấm cắt góc, ô sát góc bị giữ chỗ, hai đường chéo giao nhau, đổi đích giữa đoạn chéo, tick và lệnh. Kiểm tra bán kính quân không cắt vật cản và không chồng nhau ở các mốc nội suy 0%, 25%, 50%, 75%, 100%; thử 100 quân qua khe và so checksum replay. |
| `Assets/Game/Tests/Editor/Simulation/PlayerEconomyTests.cs` | Kiểm tra thanh toán nguyên vẹn, từ chối số âm/tràn số, sức chứa, spawn tính dân số, lệnh đúng tick và đúng chủ, chặn lệnh thử mặc định, sequence chung, thứ tự lệnh và checksum ví. |
| `Assets/Game/Tests/Editor/Simulation/ResourceNodeTests.cs` | Kiểm tra A* vòng nguồn/cấm cắt góc, rút dần/cạn/mở ô, bảo toàn địa hình, trữ lượng riêng, placement lỗi không sửa dở dang, spawn né nguồn, ID ổn định, cờ debug, tick/sequence và checksum. Quân đi được vào ô nguồn đã cạn. |
| `Assets/Game/Tests/Editor/Simulation/VillagerTests.cs` | Kiểm tra nhịp thu, vòng lặp tự động, 3 loại tài nguyên và chuyến cuối chưa đầy, thiếu nơi nộp, chọn depot đúng chủ/loại/đường, chỗ làm bị chặn, 5 Dân chia 4 ô, bảo toàn tài nguyên/khoảng cách, quyền điều khiển, đổi nguồn, Move ngắt việc, ví tràn, footprint lỗi và checksum. |
| `Assets/Game/Tests/Editor/Simulation/VillagerRecoveryTests.cs` | Kiểm tra lỗi ngắt giao hàng để xây: đủ/chưa đầy ba lô, nhớ nguồn, nộp sau xây, không có nguồn/nơi nộp, Move hủy tự động; nguồn cạn tìm cùng loại/gần/đến được, retry, phân xử hòa ổn định, nhiều Dân và bảo toàn tài nguyên qua kịch bản replay mới. |
| `Assets/Game/Tests/Editor/Simulation/BuildingTests.cs` | Preview không đổi world, footprint/cạnh map, quân/đích/ô chéo bị giữ, kiểm tra lại lúc thực thi, chi phí nguyên vẹn, quyền, đường tới móng, HP/tiến độ/sức chứa một lần, trùng lệnh, ngắt/tiếp tục xây, nhiều Dân, 3 × 3, mở chức năng nơi nộp và reroute quanh móng mới. |
| `Assets/Game/Tests/Editor/Simulation/DropOffCampTests.cs` | Dân nộp đúng xưởng xây mới cho cả ba loại, sai loại về TC, móng chưa nhận, chuyến sau dùng xưởng hoàn thiện, chờ nơi nộp rồi tự dùng xưởng mới, đo lợi ích đường ngắn và bảo toàn tiền/hàng qua kịch bản ba xưởng. |
| `Assets/Game/Tests/PlayMode/GameRTS.PlayMode.Tests.asmdef` | Assembly PlayMode tham chiếu View, Input, Simulation, UI, Commands, Data và UnityEngine.UI; chỉ đưa vào khi chạy test. |
| `Assets/Game/Tests/PlayMode/SelectionMovementTests.cs` | Nạp GameScene thật để kiểm tra click, Shift, kéo khung, ngưỡng/hủy kéo, ra lệnh cho nhóm đi vòng tường và click đích chéo làm thay đổi đồng thời X/Y trong Simulation và View. |
| `Assets/Game/Tests/PlayMode/EconomyHudTests.cs` | Nạp GameScene thật để kiểm tra ví ban đầu, 3/10 dân, nút nạp chạy sau tick, HUD đổi theo ví, chi thiếu bị từ chối, sức chứa và chặn click xuyên HUD. Kiểm tra nạp và di chuyển không trùng sequence. |
| `Assets/Game/Tests/PlayMode/ResourceNodeViewTests.cs` | Nạp scene thật: 9 nguồn chặn lưới trước khi spawn quân, nhãn đổi 100→50, cạn ẩn hình/mở ô, dữ liệu nguồn khác không đổi, quân vòng hàng cây rồi vào ô cây đã cạn. |
| `Assets/Game/Tests/PlayMode/VillagerViewTests.cs` | Scene thật: 3 Dân, TC, thu/nộp/HUD/ba lô, chọn/ngắt việc/chặn UI. Bổ sung đang giao hàng chuyển xây rồi tự nộp/thu tiếp và nguồn cạn nộp 3/10 rồi chuyển cây, cập nhật nguồn trên HUD. |
| `Assets/Game/Tests/PlayMode/BuildingViewTests.cs` | Nút xây/ghost xanh đỏ không trừ ví, trái xác nhận theo tick, sprite móng→nhà, tiến độ/búa/sức chứa, ngắt/chuột phải tiếp tục móng, giữ selection qua gesture, phải hủy không phát Move và thiếu tiền không gửi xây. |
| `Assets/Game/Tests/PlayMode/DropOffCampViewTests.cs` | Scene thật: chỉ Dân có bốn nút xây, chuyển ghost giữa các loại, từng nút xây đúng xưởng/giá và Dân nộp đúng nơi/loại, chặn click xuyên hàng nút và từ chối đặt xưởng khi thiếu tiền. |

Mở Window > General > Test Runner và chọn EditMode hoặc PlayMode để chạy test tương ứng. Menu GameRTS cũng có lệnh chạy hai bộ kiểm tra. Kết quả được ghi trong Logs; tiến độ và kết quả đã xác nhận được cập nhật trong kế hoạch gốc.

### Công cụ Editor

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Editor/GameRTS.Editor.asmdef` | Assembly chỉ dùng trong Unity Editor, tham chiếu các lớp game và API Test Runner để thiết lập scene, kiểm tra và build. |
| `Assets/Game/Editor/MilestoneOneTools.cs` | Menu thiết lập/test/build/baseline và Open GameScene có sao lưu. Nhận request qua Library/Milestone1.request, gồm ai-setup bên cạnh các bước trước và refresh; retry khóa tạm. Build Windows lấy các scene đang bật trong EditorBuildSettings, gồm GameScene/CombatScene/AiScene. Từ chối xử lý khi Play/đang biên dịch. |
| `Assets/Game/Editor/MilestoneTwoTools.cs` | Menu GameRTS > Set up Resource HUD; tạo 4 PNG icon mẫu, Prefab HUD và kết nối HUD/Input/EventSystem vào scene. Không ghi đè icon hoặc Prefab đã có, từ chối scene chưa lưu; cũng nhận action economy-setup qua công cụ Mốc 1. |
| `Assets/Game/Editor/ResourceNodeTools.cs` | Menu GameRTS > Set up Resource Nodes; tạo 3 PNG, 3 dữ liệu SO, 3 Prefab và 9 placement mẫu, nối Resource Root/F8 input. Không ghi đè asset có sẵn hoặc nhân đôi nhóm Resource Nodes; từ chối scene chưa lưu. Action resources-setup qua MilestoneOneTools. |
| `Assets/Game/Editor/VillagerTools.cs` | Menu GameRTS > Set up Villager Economy / action villagers-setup. Tạo sprite, Prefab, dữ liệu Dân/TC, thêm Worker Status vào HUD và nối 3 override Dân/TC đặt sẵn. Giữ asset, root và overrides đã có; từ chối scene chưa lưu. |
| `Assets/Game/Editor/BuildingTools.cs` | Menu GameRTS > Set up Building System / action building-setup. Tạo dữ liệu/sprite/Prefab Nhà, thêm búa và HUD xây, nối catalog, ghost và Input vào scene; giữ asset có sẵn và từ chối scene chưa lưu. |
| `Assets/Game/Editor/DropOffCampTools.cs` | Menu GameRTS > Set up Drop-off Camps / action camps-setup. Tạo ba sprite, dữ liệu và Prefab còn thiếu, thêm catalog vào GameScene, nối bốn nút xây theo ID loại và bố trí hàng nút dưới top bar. Giữ asset có sẵn, từ chối khi Play hoặc scene chưa lưu. |

### Hàng đợi huấn luyện và tập kết — bổ sung Bước 6

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Commands/TrainingCommand.cs` | Lệnh bất biến Enqueue/Cancel/SetRally có Tick/PlayerId/Sequence/BuildingId; Target là mã lượt ổn định khi hủy hoặc ô tập kết, dùng chung TC và nhà quân. Đi chung hàng đợi lệnh và bộ chống lặp. |
| `Assets/Game/Simulation/TrainingState.cs` | Định nghĩa chỉ số Dân/chi phí/thời gian/sức chứa hàng đợi bất biến; TrainingTicket lưu ID, ba loại chi phí đã trả và ProgressTicks; TrainingState giữ footprint riêng, queue chỉ đọc, cờ và trạng thái chờ ô spawn; nhà quân không cần giả làm nơi nhận tài nguyên. |
| `Assets/Game/Simulation/SimulationWorld.Training.cs` | Xác thực quyền, ví và tổng dân hiện có + queue mọi TC; thanh toán Gỗ/Thực/Vàng nguyên tử, hủy theo ID/hoàn đúng cả ba loại đã trả; mỗi tick chỉ tăng lượt đầu. Spawn ô sát TC không trùng quân/đích/ô vừa rời, tăng dân số rồi giao Move hoặc Gather theo cờ. |
| `Assets/Game/Simulation/TrainingScenario.cs` | Thế giới kiểm chứng kinh tế: 3 Dân thu Gỗ/Thực, xây Xưởng Vàng, nộp vàng và huấn luyện 3 Dân tập kết cây. Dùng chung replay Editor/Windows và test bảo toàn tài nguyên 1200 tick. |
| `Assets/Game/Data/TrainingDataSO.cs` | Cấu hình UnitDataSO, giá Thực, DurationTicks và QueueLimit; CreateDefinition sao chép sang C# thuần trước tick 1, không đưa Prefab vào Simulation. |
| `Assets/Game/Data/Buildings/VillagerTrainingData.asset` | Dân giá 50 Thực, 200 tick/10 giây, tối đa 5 lượt, tham chiếu VillagerData; được TownHall.prefab dùng. |
| `Assets/Game/Input/TrainingInput.cs` | Nhận sự kiện nút huấn luyện và hủy icon; gửi TrainingCommand sau 2 tick với sequence chung của RtsGameView. |
| `Assets/Game/UI/TrainingHudView.cs` | Đọc tên nhà/quân, giá ba loại, queue, phần trăm, dân đặt chỗ và lỗi; hiện khi chọn TC/nhà quân hoàn thiện, chặn pointer xuyên panel, phát sự kiện Train/Cancel cho Input. |
| `Assets/Game/UI/TrainingQueueSlotView.cs` | Mỗi slot giữ ID lượt đang hiển thị, icon Dân và số thứ tự; chuột phải phát ID hủy để không nhầm lượt khi queue dịch lên. |
| `Assets/Game/Art/UI/TrainingRing.png` | Vòng trắng trong suốt 64 × 64, PPU64; Image Filled/Radial360 tô màu tiến độ ở HUD. |
| `Assets/Game/Art/UI/RallyFlag.png` | Cờ xanh trong suốt 32 × 32, PPU32; SpriteRenderer con của TC chỉ hiện tại ô tập kết khi chọn TC. |
| `Assets/Game/Editor/TrainingTools.cs` | Menu GameRTS > Set up Villager Training / action training-setup; tạo dữ liệu/ảnh/marker/HUD còn thiếu, nối Input/scene, đặt ván đầu 3 Dân. Từ chối setup khi Play hoặc scene chưa lưu. |
| `Assets/Game/Tests/Editor/Simulation/TrainingTests.cs` | Kiểm tra giá/tick, 5 lượt tuần tự, tổng dân đặt chỗ nhiều TC, từ chối không trừ ví, hủy đầu/cuối/ID cũ, hoàn tràn int, cửa ra bị chặn/chiếm, rally đất/nguồn, thứ tự lệnh và chu trình kinh tế bảo toàn tài nguyên. |
| `Assets/Game/Tests/Editor/Units/TrainingAssetTests.cs` | Kiểm tra số liệu SO trên TC, vòng chọn/cờ và 5 slot HUD có tiến độ Radial360. |
| `Assets/Game/Tests/PlayMode/TrainingViewTests.cs` | Scene thật: chọn TC, bấm UI/sinh UnitView chọn được, HUD dân số, hủy icon hoàn Thực/chặn map, cờ nguồn tự khai thác và thông báo thiếu Nhà. |

### Chiến đấu — Mốc 3, Bước 1–3

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Commands/AttackCommand.cs` | Lệnh tấn công bất biến: Tick/PlayerId/Sequence, ID nhóm sao chép/sắp xếp/loại trùng, TargetKind Unit/Building và TargetId. Không gửi chỉ số sát thương từ Input. |
| `Assets/Game/Simulation/CombatStats.cs` | Dữ liệu đáng tin cậy bất biến gồm MaxHP, Armor, AttackDamage, chu kỳ đánh, tầm đánh, tốc độ đạn và tầm tự phát hiện. Khoảng cách/tốc độ nguyên theo tỷ lệ 1000; Passive giữ tương thích kịch bản không chiến đấu. |
| `Assets/Game/Simulation/ProjectileState.cs` | Mỗi mũi tên giữ ID, phe/người bắn, mục tiêu, công/tốc độ, tick phóng, vị trí tick trước/hiện tại và phần dư bước. Đạn sống độc lập với người bắn sau khi phóng. |
| `Assets/Game/Simulation/SimulationWorld.Combat.cs` | Chọn mục tiêu, quyền sở hữu, cấm đánh phe mình; đuổi theo A*, 8 ô cận chiến/cấm xuyên góc, hồi chiêu, tự dò khi rảnh và trả đòn. Đạn truy đuổi bằng số nguyên, chỉ gây max(1,công-giáp) lúc tới đích. Chết gỡ giữ chỗ/mất ba lô/giảm dân số; phá công trình gỡ footprint/depot/queue/sức chứa và bảo vệ ID xây lại. Hash toàn bộ trạng thái giao tranh. |
| `Assets/Game/Simulation/CombatScenario.cs` | Thế giới dùng chung Editor/Windows: quân cận/xa hai phe, tường có khe, đuổi/đánh/trả đòn/tên bay/chết và phá TC; đảo thứ tự nhận lệnh vẫn cùng checksum. |
| `Assets/Game/Data/Units/MeleeData.asset` | Lính giáo mẫu: 120 HP, giáp 2, công 12, đánh mỗi 20 tick, tầm 1 ô, dò địch 6 ô, tốc độ 2; lớp Spearman gây ×2.5 trước giáp khi đánh Cavalry; dùng SampleSoldier.prefab đã có HealthBar. Số liệu để thử, chưa phải cân bằng cuối. |
| `Assets/Game/Data/Units/ArcherData.asset` | Cung thủ: 45 HP, giáp 0, công 18, đánh mỗi 12 tick, tầm 6 ô, đạn 10000 đơn vị/giây, dò địch 6 ô; lớp Archer; dùng Archer.prefab. |
| `Assets/Game/View/HealthBarView.cs` | Đọc HP/max, tô thanh xanh cho player1 và đỏ cho địch, hiện số HP; không tự gây sát thương. Dùng trên quân, TC, Nhà và xưởng. |
| `Assets/Game/View/ProjectileView.cs` | Nội suy vị trí tên giữa hai tick, xoay theo hướng bay; float chỉ phục vụ hiển thị. |
| `Assets/Game/View/CombatPreviewSpawner.cs` | Chỉ có ở CombatScene, Start sau Dân: thêm giáo/cung player1 ở (1.5,4.5)/(1.5,2.5), player2 ở (10.5,2.5)/(12.5,4.5), rồi kị player1 ở (1.5,-0.5) và player2 ở (12.5,-0.5). Mọi trạng thái vẫn được World cấp ID và giữ ô. |
| `Assets/Game/View/Prefabs/Archer.prefab` | Khuôn cung thủ dùng thân quân mẫu, thêm cung/dây, vòng chọn, HealthBar và chớp đòn đánh. |
| `Assets/Game/View/Prefabs/Arrow.prefab` | ProjectileView + SpriteRenderer dùng Arrow.png, sorting35; không Collider/Rigidbody. RtsGameView lấy/trả View qua pool. |
| `Assets/Game/Art/Units/Arrow.png` | Sprite mũi tên mẫu 32 × 8 RGBA trong suốt, PPU32/Point, tạo bằng mã Editor. |
| `Assets/Game/Scenes/CombatScene.unity` | Bản đồ thử giao tranh tạo từ map kinh tế, dùng chung Prefab/SO/lõi Simulation. Khởi đầu 3 Dân + 3 quân player1 (6/10), 3 quân player2, TC địch (12,-5); camera tâm (4,0). Có điều kiện phá TC và bảng thắng/thua/chơi lại; chưa có AI kinh tế. |
| `Assets/Game/UI/SceneSwitchView.cs` | Nút THỬ CHIẾN ĐẤU/VỀ BẢN ĐỒ KINH TẾ và nút ĐẤU VỚI MÁY nạp CombatScene/GameScene/AiScene; luôn bắt đầu ván mới. Bounds cả hai nút chặn click xuyên map. AiScene ẩn nút vào chính nó. |
| `Assets/Game/Editor/CombatTools.cs` | Menu GameRTS > Set up Combat/action combat-setup: thêm HealthBar, tạo Arrow/Archer/2 SO, scene thử và nút chuyển map, nối player/prefab. Giữ dữ liệu quân đã tồn tại; gán lại liên kết SO sau khi mở scene để tránh tham chiếu rỗng. Từ chối setup khi Play hoặc GameScene chưa lưu. |
| `Assets/Game/Tests/Editor/Simulation/CombatTests.cs` | Kiểm tra giáp/cooldown, dò/đuổi/trả đòn, góc tường, thời điểm tên chạm/mục tiêu di động/người bắn chết, chết giữa đoạn chéo/ba lô/dân số/ID cũ, lệnh nhóm có người chết, phá TC/queue, nhà/sức chứa/xây lại, xây không hồi mất sát thương và replay. |
| `Assets/Game/Tests/Editor/Units/CombatAssetTests.cs` | Kiểm tra số liệu hai SO quân/HealthBar và Prefab tên có hình nhưng không Physics. |
| `Assets/Game/Tests/PlayMode/CombatViewTests.cs` | Scene thật: phân phe/cấm chọn địch, phải lên quân/nhà địch, tên hiển thị rồi thanh HP tụt, Dân chết ẩn/giảm HUD/bỏ chọn và nút chuyển về map kinh tế. |

### Quân sự và kết thúc ván — Mốc 3, Bước 4–6

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Simulation/SimulationWorld.Match.cs` | ConfigureTownCenterVictory chọn hai TC đối đầu trước tick 1; đánh giá cuối tick, chốt WinnerId/MatchEndTick và kết quả theo người chơi, hòa khi cùng sập. Step và Submit dừng sau kết thúc; hash cả cấu hình/kết quả. |
| `Assets/Game/Simulation/MilitaryScenario.cs` | Hai kịch bản replay xây ba nhà 150 Gỗ/200 công, huấn luyện/tập kết ba loại bằng giá thật, đánh TC và chốt thắng/thua trong 600 tick; hỗ trợ đảo thứ tự nhận lệnh. TC rút HP cho test, không thay asset gameplay. |
| `Assets/Game/UI/MatchHudView.cs` | Overlay VICTORY/DEFEAT/DRAW kèm tiếng Việt, chặn pointer và nút CHƠI LẠI nạp lại scene hiện tại. Chỉ đọc kết quả World; GameScene kinh tế không bật điều kiện hai TC. |
| `Assets/Game/Editor/MilitaryTools.cs` | Menu GameRTS > Set up Military and Victory/action military-setup: cập nhật ba bộ chỉ số/lớp, tạo cavalry/nhà quân/giá huấn luyện, bảy nút xây hai hàng, HUD dùng chung và bảng kết thúc; nối hai scene, bật thắng/thua ở CombatScene. Giữ nhà/quân asset mới đã có, không nhân đôi catalog. |
| `Assets/Game/Data/Units/CavalryData.asset` | Kị binh lớp Cavalry: 180 HP, giáp 1, công 16, chu kỳ 20 tick, tốc độ 6, tầm 1, dò 6; tham chiếu Cavalry.prefab. |
| `Assets/Game/View/Prefabs/Cavalry.prefab` | Sprite kị binh, UnitView/vòng chọn/HealthBar/chớp đòn đánh dùng chung; không Physics. |
| `Assets/Game/Art/Units/Cavalry.png` | Sprite ngựa/người cưỡi mẫu 64 × 64 RGBA/PPU64/Point, tạo bằng mã Editor. |
| `Assets/Game/Data/Buildings/BarracksData.asset` | Trại lính 2 × 2, 150 Gỗ, 200 công, 600 HP/giáp2, không tăng sức chứa/nhận hàng; hoàn thành mở BarracksTrainingData. |
| `Assets/Game/Data/Buildings/ArcheryRangeData.asset` | Trường bắn 2 × 2, 150 Gỗ, 200 công, 600 HP/giáp2, chỉ huấn luyện cung qua ArcheryRangeTrainingData. |
| `Assets/Game/Data/Buildings/StableData.asset` | Chuồng ngựa 2 × 2, 150 Gỗ, 200 công, 600 HP/giáp2, chỉ huấn luyện kị qua StableTrainingData. |
| `Assets/Game/Data/Buildings/BarracksTrainingData.asset` | MeleeData (giáo), 50 Thực + 25 Gỗ, 200 tick, queue5; hủy hoàn nguyên giá đã trả. |
| `Assets/Game/Data/Buildings/ArcheryRangeTrainingData.asset` | ArcherData, 25 Gỗ + 45 Vàng, 200 tick, queue5; cùng cơ chế giữ chỗ/hoàn tiền/rally. |
| `Assets/Game/Data/Buildings/StableTrainingData.asset` | CavalryData, 70 Thực + 60 Vàng, 200 tick, queue5; quân không có ba lô. |
| `Assets/Game/View/Prefabs/Buildings/Barracks.prefab` | BuildingView dùng móng chung, hình Trại lính, HP/tiến độ, Selection Marker và Rally Flag; dữ liệu từ BarracksData. |
| `Assets/Game/View/Prefabs/Buildings/ArcheryRange.prefab` | BuildingView/HP/tiến độ/vòng chọn/cờ của Trường bắn; dùng ArcheryRangeData. |
| `Assets/Game/View/Prefabs/Buildings/Stable.prefab` | BuildingView/HP/tiến độ/vòng chọn/cờ của Chuồng ngựa; dùng StableData. |
| `Assets/Game/Art/Buildings/Barracks.png` | Hình trại lính mái đỏ/cờ đỏ/giá giáo, 128 × 128 RGBA/PPU64/Point. |
| `Assets/Game/Art/Buildings/ArcheryRange.png` | Hình trường bắn mái xanh/cờ xanh/bia, 128 × 128 RGBA/PPU64/Point. |
| `Assets/Game/Art/Buildings/Stable.png` | Hình chuồng ngựa mái nâu/cờ vàng/ngựa, 128 × 128 RGBA/PPU64/Point. |
| `Assets/Game/Tests/Editor/Simulation/MilitaryTests.cs` | Kiểm tra nhân 5/2 trước giáp/giới hạn thưởng, ba trận khắc chế, móng không huấn luyện, không nhận hàng, giá ba loại/hoàn nguyên tử/tràn ví, sinh quân/rally, bị phá mất queue và hai vòng quân sự thắng/thua/replay. |
| `Assets/Game/Tests/Editor/Simulation/MatchTests.cs` | Thắng/thua/hòa, TC còn sống chưa kết thúc, dừng tick/lệnh/checksum sau kết thúc và cấu hình thắng/thua nằm trong hash. |
| `Assets/Game/Tests/Editor/Units/MilitaryAssetTests.cs` | Kiểm tra ba nhà/giá xây/giá quân/lớp/queue/thời gian, marker/cờ, ba nút xây và overlay cuối ván. |
| `Assets/Game/Tests/PlayMode/MilitaryViewTests.cs` | Scene thật: cả ba nút xây/huấn luyện/rally đúng quân, phải vào slot hoàn Gỗ+Thực, phá mỗi phía hiện đúng Victory/Defeat, khóa đầu vào/tick và Chơi lại trở về ván mới. |

### AI và tầm nhìn — Mốc 4, Bước 1–3

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Simulation/AiState.cs` | AiPhase Economy/Military/Attack; AiSettings bất biến, trạng thái nhịp quyết định/sequence/tập kết/trinh sát/nhóm quân và trí nhớ nguồn đã thấy. LastCommands chỉ phục vụ chẩn đoán, không điều khiển quyết định tiếp theo. |
| `Assets/Game/Simulation/AiController.cs` | AI dùng Command chuẩn: chia Dân 40/40/20 bằng làm tròn có tổng đúng; giữ việc đang thu, đẻ Dân, xây Nhà/xưởng/nhà quân, tiếp tục móng mất thợ, huấn luyện cân bằng ba loại, tập kết và tìm/đánh mục tiêu đã thấy. Không gọi nạp ví, trừ máu, sinh quân hoặc xây tức thì. |
| `Assets/Game/Simulation/SimulationWorld.Ai.cs` | Đăng ký AI vào TC đúng chủ trước tick1, gọi Think theo tick và hash toàn bộ trạng thái/config/trí nhớ/nhóm quân ảnh hưởng quyết định. AI dừng cùng trận; không tự tạo ví hoặc cấp quân khi đăng ký. |
| `Assets/Game/Simulation/SimulationWorld.Vision.cs` | Tầm nhìn hình tròn bằng số nguyên cho mọi player (quân 8 ô, nhà hoàn thiện 10 ô mặc định), lưu ô đã khám phá. Dùng chung để chặn lệnh vào mục tiêu chưa thấy/đặt móng ngoài tầm nhìn và lọc hiển thị. Chưa có texture màn sương hoặc che khuất theo vật cản. |
| `Assets/Game/Simulation/AiScenario.cs` | Bản đồ 64×64 và bố trí hai căn cứ/tài nguyên đối xứng dùng chung scene và kiểm thử/replay. Hai bên cùng 200 Gỗ/200 Thực/100 Vàng, TC và 3 Dân; AI2 phát triển bằng giá/tốc độ gameplay thật. |
| `Assets/Game/Data/AiProfileSO.cs` | Dữ liệu nhịp nghĩ, mục tiêu Dân, ngưỡng sang quân sự, cỡ đợt quân, giới hạn quân và bán kính xây căn cứ. CreateSettings sao chép sang C# thuần trước tick1. |
| `Assets/Game/Data/AiStandard.asset` | Cấu hình mẫu: 30 tick/1,5 giây; tối đa 10 Dân, sang quân sự từ 8 Dân, đợt 6 quân, giới hạn 18 quân chiến đấu, xây trong 10 ô quanh TC. Đây là một profile mẫu, chưa phải ba độ khó. |
| `Assets/Game/View/AiOpponentView.cs` | Start sau Dân người chơi: spawn đúng 3 Dân AI ban đầu, bật tầm nhìn chung và đăng ký AI2/profile vào TC. Không cập nhật AI theo frame và không thay đổi quyết định từ View. |
| `Assets/Game/Scenes/AiScene.unity` | Map chơi với AI, hai TC/6 Dân ban đầu và 48 nguồn hữu hạn đối xứng; không đặt sẵn quân hoặc nhà quân cho AI. Bật điều kiện thắng/thua; tắt các Command debug ví/nguồn. |
| `Assets/Game/Editor/AiTools.cs` | Menu Set up AI opponent/action ai-setup: tạo profile/AiScene còn thiếu, thêm nút ĐẤU VỚI MÁY vào HUD và scene build. Giữ AiScene/profile đã tồn tại để bảo toàn chỉnh sửa; dừng khi Play hoặc scene hiện tại chưa lưu, bàn giao về GameScene. |
| `Assets/Game/Tests/Editor/Simulation/AiTests.cs` | Kiểm tra tỷ lệ Dân cả công thức lẫn việc thật; nhịp tick; chi đúng giá/dân số; không bị ví/mỏ bí mật ảnh hưởng; lệnh/tự dò bị giới hạn tầm nhìn; cứu móng mất thợ và gọi lính lẻ về tập kết; vòng kinh tế→quân sự→tấn công, bảo toàn tài nguyên và checksum hai thế giới. |
| `Assets/Game/Tests/Editor/Units/AiAssetTests.cs` | Kiểm tra profile chuẩn, AiScene có trong build và nút HUD được nối đúng. |
| `Assets/Game/Tests/PlayMode/AiViewTests.cs` | Scene thật: khởi đầu công bằng, ẩn địch ngoài tầm nhìn, AI tự xây/huấn luyện/tấn công/kết thúc, Restart tạo AI mới và nút đi/về map kinh tế. |

## 6. File có sẵn từ template Universal 2D

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Scenes/SampleScene.unity` | Scene mẫu ban đầu của Unity; scene gameplay hiện dùng là Assets/Game/Scenes/GameScene.unity. |
| `Assets/InputSystem_Actions.inputactions` | Asset Input Actions có sẵn, được tham chiếu trong EditorBuildSettings. RtsCameraInput hiện đọc trực tiếp Keyboard/Mouse của Input System. |
| `Assets/Settings/UniversalRP.asset` | Asset cấu hình Universal Render Pipeline, liên kết renderer và các thông số dựng hình. |
| `Assets/Settings/Renderer2D.asset` | Cấu hình renderer 2D của URP, phục vụ hiển thị sprite và Tilemap. |
| `Assets/Settings/Lit2DSceneTemplate.scenetemplate` | Mẫu tạo scene Lit 2D URP, tham chiếu scene template bên dưới và có dữ liệu ảnh xem trước nhúng. |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity` | Scene nền cho template Lit 2D URP, có camera Orthographic và Global Light 2D. |
| `Assets/DefaultVolumeProfile.asset` | Volume Profile mặc định cho các thiết lập hiệu ứng hình ảnh của template. |
| `Assets/UniversalRenderPipelineGlobalSettings.asset` | Cấu hình URP ở cấp toàn project, dùng cùng GraphicsSettings và các asset dựng hình. |

Các file này là tài nguyên và thiết lập Unity. Trước khi chỉnh hoặc bỏ một asset, kiểm tra nơi tham chiếu để tránh mất cấu hình dựng hình hoặc input.

## 7. Packages và ProjectSettings

| File | Chức năng |
| --- | --- |
| `Packages/manifest.json` | Danh sách package project yêu cầu, gồm Input System, URP, công cụ 2D và Unity Test Framework. |
| `Packages/packages-lock.json` | Ghi phiên bản và phụ thuộc package đã được Unity giải quyết để giữ môi trường nhất quán khi mở project. |
| `ProjectSettings/ProjectVersion.txt` | Ghi phiên bản Editor của project, hiện là Unity 6000.3.13f1. |
| `ProjectSettings/ProjectSettings.asset` | Player Settings: cấu hình ứng dụng, nền tảng, build và các tùy chọn runtime. |
| `ProjectSettings/EditorBuildSettings.asset` | Danh sách scene build, hiện bật GameScene, CombatScene và AiScene; giữ tham chiếu cấu hình Input Actions. |
| `ProjectSettings/EditorSettings.asset` | Thiết lập Editor cho project, gồm cách quản lý và lưu asset. |
| `ProjectSettings/GraphicsSettings.asset` | Cấu hình dựng hình chung và tham chiếu render pipeline. |
| `ProjectSettings/QualitySettings.asset` | Các mức chất lượng dựng hình và thiết lập theo nền tảng. |
| `ProjectSettings/URPProjectSettings.asset` | Thiết lập ở cấp project do gói Universal Render Pipeline quản lý. |
| `ProjectSettings/ShaderGraphSettings.asset` | Thiết lập công cụ Shader Graph. |
| `ProjectSettings/TagManager.asset` | Tags, Layers và Sorting Layers dùng để phân loại đối tượng và sắp xếp hiển thị. |
| `ProjectSettings/TimeManager.asset` | Cấu hình thời gian của Unity; nhịp Simulation hiện là 20 tick/giây do SimulationWorld và RtsGameView quản lý riêng. |
| `ProjectSettings/InputManager.asset` | Cấu hình Input Manager cũ do Unity lưu; mã camera hiện dùng Input System. |
| `ProjectSettings/Physics2DSettings.asset` | Cấu hình Unity Physics 2D; không dùng để quyết định trạng thái Simulation theo kiến trúc đã chốt. |
| `ProjectSettings/DynamicsManager.asset` | Cấu hình Unity Physics 3D. |
| `ProjectSettings/NavMeshAreas.asset` | Các vùng và chi phí NavMesh của Unity; tìm đường RTS dự kiến triển khai trong Simulation. |
| `ProjectSettings/AudioManager.asset` | Cấu hình âm thanh chung của Unity. |
| `ProjectSettings/MemorySettings.asset` | Cấu hình bộ nhớ và các tùy chọn cấp phát của Unity. |
| `ProjectSettings/PackageManagerSettings.asset` | Thiết lập Package Manager cho project. |
| `ProjectSettings/PresetManager.asset` | Quản lý preset mặc định cho component và importer. |
| `ProjectSettings/SceneTemplateSettings.json` | Cấu hình công cụ scene template của Editor. |
| `ProjectSettings/VersionControlSettings.asset` | Chế độ tích hợp quản lý phiên bản trong Unity; repository Git nằm ở gốc GameRTS. |
| `ProjectSettings/UnityConnectSettings.asset` | Cấu hình tích hợp các dịch vụ Unity. |
| `ProjectSettings/MultiplayerManager.asset` | Cấu hình do công cụ multiplayer của Unity lưu; không thể hiện việc mạng lockstep của game đã được triển khai. |
| `ProjectSettings/NetworkManager.asset` | Thiết lập NetworkManager của Unity; giao thức truyền lệnh của game vẫn thuộc mốc multiplayer trong kế hoạch. |
| `ProjectSettings/ClusterInputManager.asset` | Cấu hình Cluster Input của Unity, không phải mã đọc chuột/bàn phím RTS hiện tại. |
| `ProjectSettings/VFXManager.asset` | Thiết lập hệ thống hiệu ứng VFX. |
| `ProjectSettings/XRSettings.asset` | Cấu hình XR do Unity lưu; nền tảng phát hành đã chốt của game là PC. |

## 8. Các phần đang kết nối với nhau

Luồng camera hiện tại:

```text
WASD / phím mũi tên / con lăn / chuột sát mép
    → RtsCameraInput trên Main Camera
    → RtsCameraView.ApplyInput
    → Camera pan và zoom
    → LateUpdate clamp vùng nhìn theo Ground
```

Luồng tạo quân trong GameScene:

```text
GameScene → Unit Preview → UnitPreviewSpawner.Start
    → đọc VillagerData.asset (GameScene) hoặc SO quân cấu hình
    → RtsGameView chuẩn hóa chỉ số và gọi SimulationWorld.AddUnit
    → Simulation cấp ID, tìm ô spawn trống gần vị trí cấu hình
    → tạo 3 bản sao Villager.prefab trong GameScene; CombatPreviewSpawner thêm quân hai phe ở CombatScene
    → UnitView.Render hiển thị trạng thái từ Simulation
```

Luồng chọn và ra lệnh:

```text
Chuột trái / kéo khung → UnitSelectionInput → UnitSelectionView
    → danh sách ID đang chọn → UnitView.SetSelected
Chuột phải → MoveCommand(tick, player, sequence, IDs, đích số nguyên)
    → SimulationWorld → phân ô đích → A* → giữ chỗ và di chuyển theo tick
    → RtsGameView → UnitView.Render → sprite và vòng chọn đi theo quân
```

Để đổi chỉ số loại quân, chọn asset trong Data/Units. Để đổi hình quân, mở Prefab trong View/Prefabs và thay sprite. Để đổi số lượng hoặc vị trí spawn mẫu, chọn Unit Preview trong Hierarchy và chỉnh Spawn Positions trước khi Play. Để chỉnh map, chọn Ground và dùng Tile Palette với các Tile trong Art/Terrain.

Mốc 0 và Mốc 1 đã hoàn thành. Phần di chuyển được sửa từ 4 sang 8 hướng ngày 13/11/2026: A* và bước di chuyển đều hỗ trợ đường chéo, ngăn cắt góc và giữ đúng khoảng cách di chuyển. Bộ kiểm tra mới có 42 test EditMode và 4 test PlayMode, gồm 100 quân qua khe và thay đổi cả X/Y trong scene khi ra lệnh chéo. Xem kết quả build, đối chiếu checksum Editor/Windows và nghiệm thu trong kế hoạch gốc. Phần kết nối, lobby và truyền lệnh lockstep qua mạng vẫn ở mốc multiplayer.

Mốc 2 Bước 1 đã hoàn thành: RTS Session khởi tạo ví 200 Gỗ/200 Thực/100 Vàng/10 sức chứa; 5 quân spawn sử dụng 5 dân. Bấm nút thử → EconomyTestInput → EconomyDebugCommand → hàng đợi tick chung trong SimulationWorld → PlayerEconomyState kiểm tra và thực hiện → EconomyHudView đọc kết quả. Số kiểm thử sau bổ sung là 52 EditMode và 7 PlayMode. Build PC thành công, replay Editor/Windows khớp đủ 600 tick; log theo nhóm ở mục 9, chi tiết nghiệm thu trong kế hoạch. Chức năng thu hoạch, tài nguyên trên map, xây nhà và huấn luyện chưa thuộc bước này.

Mốc 2 Bước 2 đã hoàn thành với 9 nguồn đặt sẵn: ResourceDataSO → ResourceNodeView dưới Resource Root → World.AddResource → ResourceNodeState và lớp resourceBlocked của GridMap → A* đi vòng. Rút thử theo Command → Remaining về 0 → gỡ blocker → View ẩn sprite/Stock → quân dùng được ô trống. Kiểm thử của Bước 2: 65 EditMode và 10 PlayMode; build Windows 0 lỗi, replay khớp 600 tick, đã thử chuột giữa làm cây cạn và ra lệnh cho quân vào ô cũ trên bản PC.

Mốc 2 Bước 3 đã hoàn thành: chuột phải nguồn → GatherCommand → World xác thực → Dân tìm ô cạnh nguồn → trừ nguồn/tăng ba lô theo tick → đầy hoặc nguồn cạn → tìm nơi nộp → tăng ví → tự quay lại. Tổng 3 Dân/2 lính và một TC mẫu đặt sẵn. 84 EditMode và 15 PlayMode đạt; Windows build 0 lỗi, 600 cặp checksum khớp Editor/Windows, đã thử trực tiếp một lệnh làm Dân nộp nhiều chuyến trên bản PC. Trạng thái thật và đường đi ở Simulation; UnitView/HUD chỉ hiển thị. Tạo dân và công trình kinh tế đầy đủ còn ở bước sau trong kế hoạch.

Luồng xây công trình ở Mốc 2 Bước 4:

```text
Chọn Dân → B / nút XÂY NHÀ → BuildingPlacementInput + BuildingPlacementView
    → CheckBuildingPlacement chỉ đọc → bóng xanh/đỏ và thông báo trên HUD
    → click trái gửi BuildCommand(type ID, ô gốc, IDs Dân) trễ 2 tick
    → Simulation kiểm tra lại → trừ ví → BuildingState + chặn footprint
    → Dân tìm ô cạnh móng → đi tới → mỗi Dân đứng xây tăng 1 công/tick
    → BuildingView hiện móng, thanh %, UnitView hiện búa
    → đủ công: hoàn thiện → tăng sức chứa một lần → đổi sprite Nhà
Move/Gather ngắt xây → chọn Dân, phải vào móng → ResumeBuildCommand miễn phí
```

Nhà mẫu dùng HouseData.asset (2 × 2, 50 Gỗ, 200 công, 300 HP, +5 sức chứa). Một Dân ở cạnh móng cần 10 giây, nhiều Dân góp công đồng thời. Móng chặn đường ngay khi đặt và giữ blocker khi hoàn thiện; không cho đặt lên ô quân đang dùng/giữ hoặc trên cây/công trình. HP và tiến độ ở C# thuần; ghost, búa và nội suy ở View. TC nhận hàng vẫn đặt sẵn; ba xưởng được bổ sung ở Bước 5. Huấn luyện Dân được bổ sung ở Bước 6; hủy/hoàn tiền móng chưa triển khai. Kết quả kiểm tra và bản PC ghi trong kế hoạch Markdown.

Bước 4 đã kiểm chứng với 108 EditMode/21 PlayMode đạt, build Windows 0 lỗi và 600 bộ ba checksum khớp Editor/Windows. Đã thao tác chuột trên PC: xanh/đỏ, trừ 50 Gỗ, Dân tới xây với búa/thanh tiến độ, nhà hoàn thiện và sức chứa 5/15. Lần rà soát ngày 14/11 bổ sung khôi phục việc thu: xây xong → nộp ba lô → nguồn cũ hoặc nguồn cùng loại trong 8 ô nếu đã cạn. Move hủy tự động; không có nguồn gần thì nghỉ với lý do trên HUD. Sau sửa có 122 EditMode/23 PlayMode đạt, build Windows 0 lỗi và 600 bộ bốn checksum khớp Editor/Windows. Đã thử PC: giữ 9/10 khi xây, xong nộp đủ 9 rồi quay lại thu; cây cạn tự đổi sang cây kế bên. .meta, log, cache và build được ghi theo nhóm bên dưới.

## 9. File Unity sinh tự động và sản phẩm build

Bước 5 thêm ba xưởng 2 × 2, 100 Gỗ, nhận riêng Wood/Food/Gold sau 100% xây dựng. BuildingDataSO → catalog → BuildCommand → BuildingState → hoàn thiện tạo DropOffState → StartDropOff chọn đúng chủ/loại theo đường A* ngắn nhất. TC vẫn nhận All. Bốn nút xây hiện theo lựa chọn Dân và phát ID loại qua Input; công trình không tăng dân số. 136 EditMode/27 PlayMode đạt, build Windows 0 lỗi, 600 bộ năm checksum khớp Editor/Windows. Đã thử nút Xưởng Gỗ trên PC và thu/nộp nhiều chuyến thật; Editor đã về GameScene. Danh mục bao phủ 166 file; số liệu chi tiết ở kế hoạch Markdown.

| Nhóm file hoặc thư mục | Chức năng và cách quản lý |
| --- | --- |
| `.git/` | Dữ liệu repository Git, lịch sử commit và cấu hình remote. Dùng công cụ Git hoặc GitHub Desktop để quản lý. |
| `Assets/**/*.meta` | Unity sinh file .meta cho asset và thư mục, chứa GUID và thiết lập import. Quản lý cùng asset bằng Git; giữ nguyên cặp asset/.meta khi đổi tên hoặc di chuyển. Tài liệu ghi nhóm này thay vì tạo một dòng riêng cho từng .meta tự sinh. |
| `Library/` | Cache import, assembly đã biên dịch, package tải về và dữ liệu nội bộ Unity. Git bỏ qua; không đặt mã nguồn hoặc tài nguyên tự làm vào đây. |
| `Temp/` | File tạm trong lúc Unity chạy hoặc build, Git bỏ qua. |
| `Logs/` và `*.log` | Log Unity/build, kết quả kiểm tra, nhật ký thử chuột trên PC (nhóm `Military.*`, `Ai.native-pc-*`) và bản sao runtime log. Nhóm `Ai.*` ghi kiểm thử lõi/Unity, build và replay AI Editor/Windows. Có bản sao scene tự sinh trong Logs/SceneBackups trước khi mở GameScene; GameScene.opened.txt ghi kết quả. Backup không phải scene build. Các log mốc 0 hiện còn ở gốc cũng thuộc nhóm này, Git bỏ qua. |
| `Builds/` và `Build/` | Sản phẩm build, Git bỏ qua. Bản PC hiện ở Builds/Windows/GameRTS.exe và phải đi kèm thư mục dữ liệu cùng các thành phần Unity tạo ra. |
| `UserSettings/` | Cấu hình Editor theo người dùng hoặc máy, Git bỏ qua. |
| `.vs/`, `.idea/`, `*.csproj`, `*.sln`, `*.slnx` | Cache và file project/solution IDE tự sinh; Git bỏ qua. Unity có thể tạo lại khi đồng bộ mã với IDE. |
| `Obj/`, `obj/` | File trung gian khi biên dịch, Git bỏ qua nếu xuất hiện. |

## 10. Cách cập nhật khi thêm hoặc thay đổi file

Mỗi khi thêm file được giữ lại trong dự án, bổ sung vào đúng bảng ở trên trong cùng lượt làm việc. Ghi đường dẫn, chức năng, nơi sử dụng hoặc phần liên quan. Nếu tạo nhóm chức năng mới, thêm thư mục vào sơ đồ và giải thích luồng hoạt động. File do Unity tự sinh được ghi theo các nhóm ở mục 9; file mã nguồn hoặc asset mới do dự án quản lý phải có mục riêng.

Khi di chuyển, đổi tên hoặc xóa file, sửa hoặc bỏ dòng cũ để danh mục tiếp tục khớp với project. Khi chức năng file thay đổi, cập nhật phần mô tả tương ứng. Kiểm tra lại cây thư mục, đường dẫn và quan hệ trước khi báo hoàn thành.

Tài liệu được khởi tạo và duy trì từ các file thực tế trong project. Ngày 13/11/2026 chuyển vào role, bổ sung bản Claude chỉ đọc, kế hoạch Markdown và luật bảo vệ nguyên bản. Mọi đường dẫn trong danh mục vẫn tính từ gốc GameRTS.

Bước 6 hoàn thành ngày 14/11/2026: chọn TC → nút huấn luyện → TrainingInput → TrainingCommand → World kiểm tra/trừ ví và giữ chỗ → 200 tick → spawn liền kề/tăng dân số → RtsGameView tạo UnitView → Move/Gather theo rally. Chuột phải slot gửi ID lượt để hủy/hoàn 50 Thực. Khởi đầu gameplay đã đổi thành 1 TC + 3 Dân (3/10); lính mẫu vẫn giữ để thử độc lập. 154 EditMode + 31 PlayMode đạt (185); lõi thuần 124 kiểm tra, Windows build 0 lỗi, replay phiên bản 9 đủ 600 bộ sáu checksum khớp byte. Đã thao tác trực tiếp chọn TC/huấn luyện/hủy/rally cây, quan sát 6 Dân và xưởng vàng vận hành. Danh mục hiện bao phủ 181 file duy trì; kết quả chi tiết và giới hạn ở kế hoạch Markdown.

Mốc 3 Bước 1–3: UnitDataSO → CombatStats → UnitState; Input phải lên địch → AttackCommand → PrepareCombat tìm đường → AdvanceCombat đánh hoặc phóng ProjectileState → tới đích mới trừ HP → chết/phá gỡ chặn và cập nhật kinh tế → View đọc/ẩn đối tượng. Lõi vẫn C# thuần, số nguyên 20 tick/giây. Đã đạt 143 kiểm tra lõi độc lập, 176 EditMode + 36 PlayMode (212), build Windows 0 lỗi, replay 10 đủ 600 bộ bảy checksum khớp byte. Đã mở bản PC và thao tác chọn cung/phải lên địch, quan sát giao tranh/HP/chết/dân số. Danh mục hiện có 200 file duy trì; kết quả chi tiết và giới hạn được ghi trong kế hoạch Markdown.

Bước 4–6 bổ sung: UnitClass quyết định giáo ×5/2 trước giáp khi đánh kị; các cặp còn lại khắc chế qua máu/tốc độ/tầm/nhịp đánh. Dân xây nhà quân → BuildingState đủ công → TrainingState giữ footprint riêng → chọn nhà/huấn luyện/đặt cờ → chi/hoàn ba loại → sinh quân. TC bị phá → EvaluateMatch cuối tick → đóng băng mô phỏng → MatchHudView hiện kết quả → Chơi lại nạp lại scene. Đạt171 kiểm tra lõi độc lập, 209 EditMode +39 PlayMode (248); build Windows 0 lỗi, replay11 đủ600 bộ chín checksum khớp byte. Lượt 15/11 bị lỗi kích hoạt cửa sổ nên khi đó chỉ xác minh UI qua PlayMode. Đã bổ sung thử chuột trực tiếp trên bản PC, kết thúc 16/11: xây/huấn luyện đủ ba loại bằng kinh tế thật, hủy lượt Giáo/hoàn tiền, tập kết, giao tranh/HP/chết, phá TC hiện Victory và Defeat, Chơi lại từ cả hai màn về ván mới. Không sửa code/asset hoặc build lại trong lượt nghiệm thu; nhật ký/runtime log thuộc nhóm Logs ở trên. Danh mục hiện bao phủ223 file duy trì; kết quả chi tiết và các trường hợp chưa thử thủ công được ghi trong kế hoạch.

Mốc 4 Bước 1–3 hoàn thành ngày 16/11/2026: AiProfileSO sao chép config → AiOpponentView khởi tạo công bằng/bật tầm nhìn → SimulationWorld gọi Think mỗi30 tick → AiController đọc trạng thái mình/thông tin đã thấy → Command tick+2 → luật kinh tế/xây/huấn luyện/chiến đấu hiện có → View đọc trạng thái và lọc tầm nhìn. FSM kinh tế→quân sự→tấn công giữ kinh tế chạy xuyên suốt, ngưỡng8 Dân/6 quân và mục tiêu10 Dân; 40/40/20 dùng tổng số nguyên đúng, chi phí/dân số không có ngoại lệ cho AI. Lính nhàn ở xa được gọi về tập kết. AiScene đối xứng bắt đầu mỗi bên1 TC+3 Dân, không cho sẵn nhà quân hoặc lính.

Kết quả hiện hành: 189 kiểm tra lõi độc lập, 229 EditMode +42 PlayMode (271) đạt; Windows build0 lỗi; replay12 đủ600 hàng×10 checksum khớp byte Editor/Windows (104.505 byte). Đã thử PC: vào AiScene qua nút, chờ AI tự kéo đủ ba binh chủng sang phá TC, hiện DEFEAT, Chơi lại đúng trạng thái ban đầu và quay về GameScene. Native journal/runtime log thuộc nhóm Logs ở mục9; Editor/PC bàn giao ở GameScene. Danh mục hiện bao phủ236 file duy trì; .meta tự sinh được quản lý cùng asset. Ba độ khó, rút lui chiến lược, fog đen/mờ và multiplayer vẫn chưa hoàn thành; chi tiết ở kế hoạch Markdown.
