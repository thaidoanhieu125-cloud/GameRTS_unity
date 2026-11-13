# Cấu trúc dự án GameRTS

Cập nhật ngày 14/11/2026. Tài liệu giải thích các thư mục, file hiện có và cách chúng kết nối với nhau, giúp người đang học Unity tìm đúng nơi để đọc hoặc chỉnh sửa. Mọi đường dẫn trong tài liệu được tính từ thư mục gốc `GameRTS`.

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
│   │   │   ├── Buildings/          Sprite Tòa thị chính, móng và Nhà hoàn thiện
│   │   │   └── UI/                 4 icon tài nguyên mẫu, nền trong suốt
│   │   ├── Commands/               Lệnh di chuyển, khai thác, xây và lệnh thử
│   │   ├── Data/
│   │   │   ├── Units/              Asset dữ liệu từng loại quân
│   │   │   ├── Resources/          Asset dữ liệu từng loại nguồn tài nguyên
│   │   │   └── Buildings/          Dữ liệu nơi nộp và loại công trình xây được
│   │   ├── Editor/                 Công cụ thiết lập scene, kiểm tra và build
│   │   ├── Input/                  Đọc chuột và bàn phím
│   │   ├── Scenes/                 Scene gameplay
│   │   ├── Simulation/             Lưới, A*, quân, ví từng người chơi và tick
│   │   ├── Tests/Editor/
│   │   │   ├── Simulation/         Kiểm thử A*, lệnh, va chạm và replay
│   │   │   └── Units/              Kiểm thử dữ liệu, Prefab và spawn
│   │   ├── Tests/PlayMode/          Kiểm thử chọn và điều khiển quân trong scene
│   │   ├── UI/                     HUD tài nguyên bằng uGUI, chỉ đọc Simulation
│   │   │   └── Prefabs/            EconomyHud: top bar, thử ví, trạng thái Dân và nút xây
│   │   └── View/
│   │       └── Prefabs/            Khuôn GameObject dùng để tạo đối tượng
│   │           ├── Resources/      Prefab cây, bụi quả và mỏ vàng
│   │           └── Buildings/      Tòa thị chính và Nhà có tiến độ xây
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
| `Game RTS Unity Doc.md` | Kế hoạch triển khai đang cập nhật, chuyển đủ 187 đoạn và 4 bảng từ bản DOCX hiện tại. Ghi quyết định đã chốt, thiết kế, tiến độ và kết quả kiểm tra. |
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
| `Assets/Game/Simulation/UnitState.cs` | Trạng thái quân, di chuyển và SegmentProgress. Dân có thêm VillagerState khi carryCapacity > 0; lính có Villager = null. Chứa trạng thái thật bằng C# thuần, View chỉ đọc. |
| `Assets/Game/Simulation/SimulationWorld.cs` | Mô phỏng 20 tick/giây, ví/quân/nguồn và hàng đợi Move/Gather/Build/ResumeBuild dùng chung sequence. Điều phối các phần Economy/Buildings; checksum gồm catalog, công trình, tiến độ/HP, các lớp lưới và công việc của Dân. Lệnh Move ngắt thu/xây; cờ debug độc lập với lệnh gameplay. |
| `Assets/Game/Simulation/SimulationWorld.Economy.cs` | Kinh tế trong partial class SimulationWorld: Gather, chia ô làm, chọn nơi nộp đúng chủ/loại theo đường đi; giữ ba lô, nộp chuyến cuối chưa đầy. Nguồn cạn tự tìm cùng ResourceType trong ResourceSearchRadius = 8 quanh nguồn cũ, ưu tiên đường ngắn/ID/ô đứng; SearchingForResource thử lại mỗi 20 tick khi bị chặn. Không có nguồn phù hợp thì nghỉ. |
| `Assets/Game/Simulation/BuildingState.cs` | PlacementIssue, BuildingDefinition bất biến (ID, footprint, chi phí, lượng công, HP, sức chứa, loại nhận hàng) và BuildingState giữ chủ sở hữu/ô gốc/CompletedWork/HitPoints/ProgressPermille. ID công trình = ô gốc + 1; chỉ hoàn thành mới kích hoạt chức năng. |
| `Assets/Game/Simulation/SimulationWorld.Buildings.cs` | Catalog/kiểm tra footprint và đường Dân; Build trừ ví một lần, móng chặn lưới, ResumeBuild miễn phí. Giữ ResourceId khi giao xây; EndBuildingJob tự nộp ba lô rồi quay lại thu. Tăng tiến độ/HP theo tick, hoàn thiện tăng sức chứa/kích hoạt nơi nộp đúng một lần. |
| `Assets/Game/Simulation/VillagerState.cs` | Trạng thái Dân: công việc, nguồn/nơi nộp/công trình, ba lô, WorkTicks/RetryTicks. Có nhóm xây và SearchingForResource khi chờ đường tới nguồn thay thế. ResourceId được nhớ qua việc xây; Move xóa nguồn. Các trạng thái đều tham gia checksum. |
| `Assets/Game/Simulation/DropOffState.cs` | Trạng thái bất biến nơi nộp: ID theo ô gốc + 1, chủ sở hữu, ô gốc/kích thước và mặt nạ ResourceAcceptance Wood/Food/Gold/All. Công trình chặn lưới; Dân tương tác ở ô sát cạnh footprint. |
| `Assets/Game/Simulation/PlayerEconomyState.cs` | Ví số nguyên gồm Wood/Food/Gold/Population/MaxPopulation. Nạp, thanh toán nhiều loại cùng lúc, giữ/giải phóng dân số và tăng sức chứa; từ chối số âm, thiếu tiền, vượt sức chứa hoặc tràn int mà không thay đổi dở dang. |
| `Assets/Game/Simulation/ResourceNodeState.cs` | Enum ResourceType (Wood/Food/Gold) và trạng thái C# thuần: ID, ID loại, ô chiếm, trữ lượng ban đầu/còn lại, IsDepleted. Mỗi nguồn có trạng thái riêng, không sửa ResourceDataSO khi khai thác; nguồn cạn vẫn giữ ID để lệnh cũ không tác động nhầm nguồn khác. |
| `Assets/Game/Simulation/DeterminismScenario.cs` | Replay phiên bản 7 ghi bốn checksum mỗi tick: 100 quân di chuyển, 3 Dân thu/nộp, xây công trình và Dân bị ngắt giao hàng để xây rồi tự nộp/tìm nguồn tiếp. Chạy 600 tick, không đưa FPS/ghost vào kết quả; ghi SearchRadius = 8. |
| `Assets/Game/Simulation/VillagerScenario.cs` | CreateWorld: 3 Dân thu đủ 25 mỗi loại. CreateRecoveryWorld: Dân đang mang 10 bị giao xây, sau đó tự nộp/thu phần cuối 2, tìm cây kế tiếp 8; tổng 20 Gỗ bảo toàn và chi 50 Gỗ xây Nhà. Dùng chung trong test/replay Editor và Windows. |
| `Assets/Game/Simulation/ConstructionScenario.cs` | Kịch bản xây C# thuần: 2 Dân/1 lính, nhà 2 × 2 và kho 3 × 3; Build, Move ngắt việc, ResumeBuild và công trình nhận hàng khi xong. Dùng chung cho Editor và Windows. |
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
| `Assets/Game/Data/UnitDataSO.cs` | ID, tên, tốc độ, bán kính, Prefab và nhóm Villager: Can Gather, Carry Capacity, Gather Interval Ticks. CarryCapacity đưa vào Simulation là 0 nếu tắt Can Gather; chuẩn hóa cấu hình trong OnValidate. |
| `Assets/Game/Data/Units/SampleSoldierData.asset` | Dữ liệu loại `sample_soldier`, tên Lính mẫu, tốc độ 3, bán kính 0,45; tham chiếu SampleSoldier.prefab và được Unit Preview trong GameScene sử dụng. Các chỉ số là giá trị thử nghiệm. |
| `Assets/Game/Data/Units/VillagerData.asset` | Dữ liệu Dân: ID villager, tốc độ 3, bán kính 0,45, Can Gather bật, tải 10, chu kỳ 10 tick và Villager.prefab. |
| `Assets/Game/Data/DropOffDataSO.cs` | ScriptableObject nơi nộp: tên, Width/Height 1..8 ô, ResourceAcceptance và Sprite. Menu Create > GameRTS > Drop Off Data; bộ lọc cho phép cấu hình xưởng theo từng loại. Chưa phải dữ liệu xây/huấn luyện/chiến đấu đầy đủ. |
| `Assets/Game/Data/Buildings/TownHallData.asset` | Tòa thị chính mẫu footprint 2 × 2, nhận cả 3 tài nguyên, TownHall.png; dùng bởi TownHall.prefab. |
| `Assets/Game/Data/BuildingDataSO.cs` | Dữ liệu xây: ID/tên, Width/Height, 3 chi phí, RequiredWork, MaxHitPoints, PopulationBonus, ResourceAcceptance, sprite móng/hoàn thiện và Prefab. CreateDefinition chuyển sang dữ liệu C# thuần trước tick 1. |
| `Assets/Game/Data/Buildings/HouseData.asset` | Nhà mẫu ID house, 2 × 2, 50 Gỗ, 200 công (10 giây một Dân đã tới chỗ), 300 HP, +5 sức chứa khi hoàn thiện; không nhận hàng. Tham chiếu hai sprite và House.prefab. |
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
| `Assets/Game/Input/UnitSelectionInput.cs` | Click/kéo/Shift chọn quân. Chuột phải nguồn tạo GatherCommand, chuột phải móng nhà mình chưa xong tạo ResumeBuildCommand cho Dân; quân khác giữ lệnh cũ. Chuột phải nền tạo MoveCommand và ngắt công việc. Dùng chung sequence, bỏ qua HUD/gesture đặt móng và kiểm tra focus. |
| `Assets/Game/Input/EconomyTestInput.cs` | F9 mở/đóng bảng thử ví; nhận nút bấm từ HUD, tạo EconomyDebugCommand sau 2 tick với lượng 50 mỗi loại hoặc tăng sức chứa 5. Dùng NextLocalSequence chung với di chuyển; không trực tiếp sửa ví. |
| `Assets/Game/Input/ResourceTestInput.cs` | Trỏ nguồn + F8 hoặc click chuột giữa rút thử 50; giữ Shift để rút hết. Bỏ qua HUD/ngoài viewport/nguồn cạn/chế độ đặt móng; tạo ResourceDebugCommand sau 2 tick với sequence chung. Bật/tắt theo Enable Resource Testing. |
| `Assets/Game/Input/BuildingPlacementInput.cs` | B hoặc nút Xây nhà bật/tắt ghost với Dân đang chọn; trái gửi BuildCommand, phải/Esc hủy. Điều phối preview, UI và sequence chung; API Begin/Preview/Confirm/Cancel dùng trong PlayMode test. Chuột phải móng giao ResumeBuild qua UnitSelectionInput. |

### View và Prefab

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/View/GameRTS.View.asmdef` | Định nghĩa assembly hiển thị dùng UnityEngine, tham chiếu Simulation, Commands và Data. |
| `Assets/Game/View/RtsCameraView.cs` | Gắn trên Main Camera, pan theo thời gian, zoom Orthographic và clamp toàn bộ vùng nhìn vào biên Tilemap Ground. Nhận điều khiển từ RtsCameraInput. |
| `Assets/Game/View/UnitView.cs` | Liên kết dữ liệu/ID, vòng chọn và nội suy vị trí. Dân có Cargo/marker hàng và búa quay khi đứng xây. Chỉ đọc trạng thái; thời gian hiệu ứng không quyết định thu, nộp hay tiến độ xây. |
| `Assets/Game/View/UnitPreviewSpawner.cs` | Spawn Positions và Unit Data mặc định, thêm Unit Data Overrides theo từng vị trí. Scene dùng 3 override VillagerData + 2 lính mặc định. Giữ chế độ xem Prefab độc lập khi không gán game. |
| `Assets/Game/View/RtsMapView.cs` | Gắn trên Map Grid, chuyển ô Ground có nền và không có tile ở Obstacles thành ô đi được trong GridMap lúc khởi tạo. Kiểm tra map dùng ô XY 1 × 1. |
| `Assets/Game/View/RtsGameView.cs` | Khởi tạo World, catalog BuildingDataSO, nguồn/nơi nộp trước khi spawn quân. LateUpdate render quân/nguồn và tạo/cập nhật BuildingView theo trạng thái thật; RenderBuildings cũng dùng để kiểm tra View khi tự bước tick. |
| `Assets/Game/View/BuildingView.cs` | Gắn House.prefab, đặt hình theo footprint và đọc BuildingState để đổi sprite móng/hoàn thiện, thanh tiến độ, nhãn %. Tắt thanh khi xong; không thay đổi công hoặc tài nguyên. |
| `Assets/Game/View/BuildingPlacementView.cs` | Ghost sprite cùng từng ô footprint xanh/đỏ, chỉ đọc CheckBuildingPlacement. Giữ chế độ đặt và chặn gesture chọn/di chuyển xuyên qua lần xác nhận hoặc hủy. Đây là trạng thái View cục bộ, không thuộc checksum. |
| `Assets/Game/View/DropOffView.cs` | Khởi tạo nơi nộp từ DropOffDataSO và ownerId; chuyển vị trí tâm hình thành ô gốc footprint cho World.AddDropOff. Sprite hiển thị công trình đặt sẵn; không có xây dựng theo tiến độ hoặc tạo quân ở Bước 3. |
| `Assets/Game/View/ResourceNodeView.cs` | ResourceDataSO + SpriteRenderer + TextMesh Stock. Initialize đăng ký nguồn vào World và đặt ở tâm ô; Render đọc Remaining để đổi nhãn, tắt sprite/nhãn khi cạn. Gizmo vàng hiển thị footprint 1 ô trong Scene View. Không dùng Collider2D/Unity Physics để chặn quân. |
| `Assets/Game/View/UnitSelectionView.cs` | Giữ danh sách ID được chọn tại máy người chơi. Xử lý click, kéo khung mọi hướng, Shift chọn thêm/bỏ chọn và vẽ khung quét trên màn hình. Cập nhật vòng chọn qua UnitView. |
| `Assets/Game/View/ReplayVerification.cs` | Nhận -rtsReplayCheck, ghi 600 bộ bốn checksum (di chuyển/thu hoạch/xây/khôi phục việc Dân) rồi thoát; không tác động luồng chơi bình thường. |
| `Assets/Game/View/Prefabs/SampleSoldier.prefab` | Khuôn quân mẫu gồm SpriteRenderer và UnitView ở gốc, con SelectionCircle có SpriteRenderer và mặc định tắt. Tham chiếu hai sprite trong Art/Units; UnitDataSO tham chiếu Prefab này. |
| `Assets/Game/View/Prefabs/Villager.prefab` | Prefab Dân có sprite riêng, vòng chọn, Cargo 0/10, Cargo Marker và Hammer mặc định ẩn, gán vào UnitView. Dữ liệu nằm trong VillagerData.asset. |
| `Assets/Game/View/Prefabs/Buildings/TownHall.prefab` | Prefab Tòa thị chính mẫu có DropOffView, SpriteRenderer và nhãn; chiếm 2 × 2 ô, owner1 trong scene và nhận All. |
| `Assets/Game/View/Prefabs/Buildings/House.prefab` | BuildingView với Body, Progress Background/Fill và nhãn tiến độ. Hình tự co theo footprint, hoàn thiện đổi sprite và tiếp tục giữ ô cản. |
| `Assets/Game/View/Prefabs/Resources/Tree.prefab` | Khuôn cây, SpriteRenderer với Tree.png, ResourceNodeView dùng TreeData và nhãn trữ lượng 100. |
| `Assets/Game/View/Prefabs/Resources/BerryBush.prefab` | Khuôn bụi quả, BerryBush.png/BerryBushData và nhãn 150. |
| `Assets/Game/View/Prefabs/Resources/GoldMine.prefab` | Khuôn mỏ vàng, GoldMine.png/GoldMineData và nhãn 800. |

`UnitId` trong dữ liệu xác định loại quân; `InstanceId` của UnitView trong GameScene nhận từ ID do SimulationWorld cấp. Chọn quân và góc nhìn là trạng thái cục bộ. Vị trí thật và quyết định di chuyển do Simulation quản lý.

### UI — Thanh tài nguyên

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/UI/GameRTS.UI.asmdef` | Assembly giao diện tham chiếu View, Simulation, Commands, Data và UnityEngine.UI. |
| `Assets/Game/UI/EconomyHudView.cs` | Đọc ví/top bar, phản hồi thử ví và Worker Status gồm việc/ba lô/trữ lượng/tiến độ xây. Hiện chờ đường tới nguồn thay thế hoặc hết nguồn cùng loại trong 8 ô. Các panel chặn click xuyên xuống map. |
| `Assets/Game/UI/Prefabs/EconomyHud.prefab` | Canvas 1280 × 720: 4 tài nguyên, thử ví, hướng dẫn, Worker Status, nút XÂY NHÀ và Construction Notice. Bảng Dân/thông báo chỉ hiện khi cần, chặn click xuyên vào map. |
| `Assets/Game/UI/ConstructionHudView.cs` | Nút Xây nhà, thông tin giá/kích thước, lý do không đặt được và kết quả thực thi lệnh. Construction Notice chặn chuột xuyên xuống map; chỉ phát yêu cầu cho Input và đọc kết quả từ World. |

HUD chỉ đọc ví. UnitSelectionInput bỏ qua click/chuột phải trên top bar hoặc bảng thử; RtsCameraInput bỏ qua edge scrolling và zoom khi con trỏ ở các vùng đó. WASD vẫn dùng được. EventSystem trong GameScene dùng InputSystemUIInputModule để các Button nhận được Input System mới.

## 4. Scene và hình ảnh riêng của game

| File | Chức năng và nơi sử dụng |
| --- | --- |
| `Assets/Game/Scenes/GameScene.unity` | Scene build: 3 Dân + 2 lính, 9 nguồn, ví/HUD, TC đặt sẵn tâm (2,-5), 2 × 2. RTS Session gán catalog Nhà, BuildingPlacementView/Input và Constructed Buildings để chứa nhà được xây trong ván. Mới vào Dân đứng chờ lệnh. |
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
| `Assets/Game/Tests/Editor/Simulation/GameRTS.Simulation.Tests.asmdef` | Assembly kiểm thử Editor tham chiếu Simulation và Commands để kiểm tra lõi C# mà không cần GameObject gameplay. |
| `Assets/Game/Tests/Editor/Simulation/SimulationTests.cs` | Kiểm thử A*, tốc độ ở cả 8 hướng, cấm cắt góc, ô sát góc bị giữ chỗ, hai đường chéo giao nhau, đổi đích giữa đoạn chéo, tick và lệnh. Kiểm tra bán kính quân không cắt vật cản và không chồng nhau ở các mốc nội suy 0%, 25%, 50%, 75%, 100%; thử 100 quân qua khe và so checksum replay. |
| `Assets/Game/Tests/Editor/Simulation/PlayerEconomyTests.cs` | Kiểm tra thanh toán nguyên vẹn, từ chối số âm/tràn số, sức chứa, spawn tính dân số, lệnh đúng tick và đúng chủ, chặn lệnh thử mặc định, sequence chung, thứ tự lệnh và checksum ví. |
| `Assets/Game/Tests/Editor/Simulation/ResourceNodeTests.cs` | Kiểm tra A* vòng nguồn/cấm cắt góc, rút dần/cạn/mở ô, bảo toàn địa hình, trữ lượng riêng, placement lỗi không sửa dở dang, spawn né nguồn, ID ổn định, cờ debug, tick/sequence và checksum. Quân đi được vào ô nguồn đã cạn. |
| `Assets/Game/Tests/Editor/Simulation/VillagerTests.cs` | Kiểm tra nhịp thu, vòng lặp tự động, 3 loại tài nguyên và chuyến cuối chưa đầy, thiếu nơi nộp, chọn depot đúng chủ/loại/đường, chỗ làm bị chặn, 5 Dân chia 4 ô, bảo toàn tài nguyên/khoảng cách, quyền điều khiển, đổi nguồn, Move ngắt việc, ví tràn, footprint lỗi và checksum. |
| `Assets/Game/Tests/Editor/Simulation/VillagerRecoveryTests.cs` | Kiểm tra lỗi ngắt giao hàng để xây: đủ/chưa đầy ba lô, nhớ nguồn, nộp sau xây, không có nguồn/nơi nộp, Move hủy tự động; nguồn cạn tìm cùng loại/gần/đến được, retry, phân xử hòa ổn định, nhiều Dân và bảo toàn tài nguyên qua kịch bản replay mới. |
| `Assets/Game/Tests/Editor/Simulation/BuildingTests.cs` | Preview không đổi world, footprint/cạnh map, quân/đích/ô chéo bị giữ, kiểm tra lại lúc thực thi, chi phí nguyên vẹn, quyền, đường tới móng, HP/tiến độ/sức chứa một lần, trùng lệnh, ngắt/tiếp tục xây, nhiều Dân, 3 × 3, mở chức năng nơi nộp và reroute quanh móng mới. |
| `Assets/Game/Tests/PlayMode/GameRTS.PlayMode.Tests.asmdef` | Assembly PlayMode tham chiếu View, Input, Simulation, UI, Commands, Data và UnityEngine.UI; chỉ đưa vào khi chạy test. |
| `Assets/Game/Tests/PlayMode/SelectionMovementTests.cs` | Nạp GameScene thật để kiểm tra click, Shift, kéo khung, ngưỡng/hủy kéo, ra lệnh cho nhóm đi vòng tường và click đích chéo làm thay đổi đồng thời X/Y trong Simulation và View. |
| `Assets/Game/Tests/PlayMode/EconomyHudTests.cs` | Nạp GameScene thật để kiểm tra ví ban đầu, 5/10 dân, nút nạp chạy sau tick, HUD đổi theo ví, chi thiếu bị từ chối, sức chứa và chặn click xuyên HUD. Kiểm tra nạp và di chuyển không trùng sequence. |
| `Assets/Game/Tests/PlayMode/ResourceNodeViewTests.cs` | Nạp scene thật: 9 nguồn chặn lưới trước khi spawn quân, nhãn đổi 100→50, cạn ẩn hình/mở ô, dữ liệu nguồn khác không đổi, quân vòng hàng cây rồi vào ô cây đã cạn. |
| `Assets/Game/Tests/PlayMode/VillagerViewTests.cs` | Scene thật: 3 Dân/2 lính, TC, thu/nộp/HUD/ba lô, chọn/ngắt việc/chặn UI. Bổ sung đang giao hàng chuyển xây rồi tự nộp/thu tiếp và nguồn cạn nộp 3/10 rồi chuyển cây, cập nhật nguồn trên HUD. |
| `Assets/Game/Tests/PlayMode/BuildingViewTests.cs` | Nút xây/ghost xanh đỏ không trừ ví, trái xác nhận theo tick, sprite móng→nhà, tiến độ/búa/sức chứa, ngắt/chuột phải tiếp tục móng, giữ selection qua gesture, phải hủy không phát Move và thiếu tiền không gửi xây. |

Mở Window > General > Test Runner và chọn EditMode hoặc PlayMode để chạy test tương ứng. Menu GameRTS cũng có lệnh chạy hai bộ kiểm tra. Kết quả được ghi trong Logs; tiến độ và kết quả đã xác nhận được cập nhật trong kế hoạch gốc.

### Công cụ Editor

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Editor/GameRTS.Editor.asmdef` | Assembly chỉ dùng trong Unity Editor, tham chiếu các lớp game và API Test Runner để thiết lập scene, kiểm tra và build. |
| `Assets/Game/Editor/MilestoneOneTools.cs` | Menu thiết lập scene, test EditMode/PlayMode, build Windows và baseline replay. Nhận tác vụ qua Library/Milestone1.request, gồm building-setup và refresh để nạp asset/mã mới trong Editor đang mở. Giữ Obstacles có sẵn, từ chối setup scene chưa lưu. |
| `Assets/Game/Editor/MilestoneTwoTools.cs` | Menu GameRTS > Set up Resource HUD; tạo 4 PNG icon mẫu, Prefab HUD và kết nối HUD/Input/EventSystem vào scene. Không ghi đè icon hoặc Prefab đã có, từ chối scene chưa lưu; cũng nhận action economy-setup qua công cụ Mốc 1. |
| `Assets/Game/Editor/ResourceNodeTools.cs` | Menu GameRTS > Set up Resource Nodes; tạo 3 PNG, 3 dữ liệu SO, 3 Prefab và 9 placement mẫu, nối Resource Root/F8 input. Không ghi đè asset có sẵn hoặc nhân đôi nhóm Resource Nodes; từ chối scene chưa lưu. Action resources-setup qua MilestoneOneTools. |
| `Assets/Game/Editor/VillagerTools.cs` | Menu GameRTS > Set up Villager Economy / action villagers-setup. Tạo sprite, Prefab, dữ liệu Dân/TC, thêm Worker Status vào HUD và nối 3 override Dân/TC đặt sẵn. Giữ asset, root và overrides đã có; từ chối scene chưa lưu. |
| `Assets/Game/Editor/BuildingTools.cs` | Menu GameRTS > Set up Building System / action building-setup. Tạo dữ liệu/sprite/Prefab Nhà, thêm búa và HUD xây, nối catalog, ghost và Input vào scene; giữ asset có sẵn và từ chối scene chưa lưu. |

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
| `ProjectSettings/EditorBuildSettings.asset` | Danh sách scene build, hiện bật GameScene; đồng thời giữ tham chiếu cấu hình Input Actions. |
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
    → đọc SampleSoldierData.asset
    → RtsGameView chuẩn hóa chỉ số và gọi SimulationWorld.AddUnit
    → Simulation cấp ID, tìm ô spawn trống gần vị trí cấu hình
    → tạo 5 bản sao SampleSoldier.prefab và gán ID tương ứng
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

Nhà mẫu dùng HouseData.asset (2 × 2, 50 Gỗ, 200 công, 300 HP, +5 sức chứa). Một Dân ở cạnh móng cần 10 giây, nhiều Dân góp công đồng thời. Móng chặn đường ngay khi đặt và giữ blocker khi hoàn thiện; không cho đặt lên ô quân đang dùng/giữ hoặc trên cây/công trình. HP và tiến độ ở C# thuần; ghost, búa và nội suy ở View. TC nhận hàng vẫn đặt sẵn; nút xây ba xưởng, tạo dân và hủy/hoàn tiền móng chưa triển khai. Kết quả kiểm tra và bản PC ghi trong kế hoạch Markdown.

Bước 4 đã kiểm chứng với 108 EditMode/21 PlayMode đạt, build Windows 0 lỗi và 600 bộ ba checksum khớp Editor/Windows. Đã thao tác chuột trên PC: xanh/đỏ, trừ 50 Gỗ, Dân tới xây với búa/thanh tiến độ, nhà hoàn thiện và sức chứa 5/15. Lần rà soát ngày 14/11 bổ sung khôi phục việc thu: xây xong → nộp ba lô → nguồn cũ hoặc nguồn cùng loại trong 8 ô nếu đã cạn. Move hủy tự động; không có nguồn gần thì nghỉ với lý do trên HUD. Sau sửa có 122 EditMode/23 PlayMode đạt, build Windows 0 lỗi và 600 bộ bốn checksum khớp Editor/Windows. Đã thử PC: giữ 9/10 khi xây, xong nộp đủ 9 rồi quay lại thu; cây cạn tự đổi sang cây kế bên. Danh mục hiện bao phủ 153 file được dự án quản lý; .meta, log, cache và build được ghi theo nhóm bên dưới.

## 9. File Unity sinh tự động và sản phẩm build

| Nhóm file hoặc thư mục | Chức năng và cách quản lý |
| --- | --- |
| `.git/` | Dữ liệu repository Git, lịch sử commit và cấu hình remote. Dùng công cụ Git hoặc GitHub Desktop để quản lý. |
| `Assets/**/*.meta` | Unity sinh file .meta cho asset và thư mục, chứa GUID và thiết lập import. Quản lý cùng asset bằng Git; giữ nguyên cặp asset/.meta khi đổi tên hoặc di chuyển. Tài liệu ghi nhóm này thay vì tạo một dòng riêng cho từng .meta tự sinh. |
| `Library/` | Cache import, assembly đã biên dịch, package tải về và dữ liệu nội bộ Unity. Git bỏ qua; không đặt mã nguồn hoặc tài nguyên tự làm vào đây. |
| `Temp/` | File tạm trong lúc Unity chạy hoặc build, Git bỏ qua. |
| `Logs/` và `*.log` | Log Unity, build và kết quả kiểm tra. Các log mốc 0 hiện còn ở gốc cũng thuộc nhóm này, Git bỏ qua. |
| `Builds/` và `Build/` | Sản phẩm build, Git bỏ qua. Bản PC hiện ở Builds/Windows/GameRTS.exe và phải đi kèm thư mục dữ liệu cùng các thành phần Unity tạo ra. |
| `UserSettings/` | Cấu hình Editor theo người dùng hoặc máy, Git bỏ qua. |
| `.vs/`, `.idea/`, `*.csproj`, `*.sln`, `*.slnx` | Cache và file project/solution IDE tự sinh; Git bỏ qua. Unity có thể tạo lại khi đồng bộ mã với IDE. |
| `Obj/`, `obj/` | File trung gian khi biên dịch, Git bỏ qua nếu xuất hiện. |

## 10. Cách cập nhật khi thêm hoặc thay đổi file

Mỗi khi thêm file được giữ lại trong dự án, bổ sung vào đúng bảng ở trên trong cùng lượt làm việc. Ghi đường dẫn, chức năng, nơi sử dụng hoặc phần liên quan. Nếu tạo nhóm chức năng mới, thêm thư mục vào sơ đồ và giải thích luồng hoạt động. File do Unity tự sinh được ghi theo các nhóm ở mục 9; file mã nguồn hoặc asset mới do dự án quản lý phải có mục riêng.

Khi di chuyển, đổi tên hoặc xóa file, sửa hoặc bỏ dòng cũ để danh mục tiếp tục khớp với project. Khi chức năng file thay đổi, cập nhật phần mô tả tương ứng. Kiểm tra lại cây thư mục, đường dẫn và quan hệ trước khi báo hoàn thành.

Tài liệu được khởi tạo và duy trì từ các file thực tế trong project. Ngày 13/11/2026 chuyển vào role, bổ sung bản Claude chỉ đọc, kế hoạch Markdown và luật bảo vệ nguyên bản. Mọi đường dẫn trong danh mục vẫn tính từ gốc GameRTS.
