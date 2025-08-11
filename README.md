# 🏃‍♂️ Animal Rush

**Animal Rush** là một game endless runner 2D thú vị được phát triển bằng Unity, nơi bạn điều khiển các con vật đáng yêu chạy qua những thử thách đầy nguy hiểm!

![Game Banner](https://img.shields.io/badge/Unity-2022.3+-blue.svg)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Mobile-brightgreen.svg)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow.svg)

## 🎮 Tổng Quan Game

Animal Rush là một trải nghiệm chạy vô tận đầy thách thức, nơi người chơi điều khiển các con vật với khả năng đặc biệt để vượt qua các chướng ngại vật, thu thập coin và đạt điểm số cao nhất có thể.

### ✨ Tính Năng Chính

- 🐾 **Đa dạng nhân vật**: Nhiều con vật khác nhau với khả năng riêng biệt
- 🏃‍♀️ **Endless Running**: Gameplay chạy vô tận với độ khó tăng dần
- 💰 **Hệ thống Coin**: Thu thập coin để mở khóa nhân vật mới
- ⭐ **Abilities đặc biệt**: Double jump, magnet, shield, speed boost
- 🎯 **Obstacles đa dạng**: Hộp, hố, kẻ thù, chim bay
- 💖 **Hệ thống Lives**: Quản lý mạng sống thông minh
- 🏆 **Leaderboard**: Theo dõi điểm số cao nhất
- 🎵 **Âm thanh**: Hiệu ứng âm thanh và nhạc nền

## 🎯 Cách Chơi

### Điều Khiển
- **Space** hoặc **Chuột trái**: Nhảy
- **A/D** hoặc **Mũi tên**: Di chuyển trái/phải
- **ESC**: Tạm dừng game

### Mục Tiêu
1. Chạy càng xa càng tốt
2. Thu thập nhiều coin nhất có thể
3. Tránh các chướng ngại vật
4. Mở khóa tất cả nhân vật
5. Đạt điểm số cao trong leaderboard

## 🐾 Nhân Vật & Abilities

Mỗi con vật trong Animal Rush có những khả năng đặc biệt riêng:

### Abilities Có Thể Có:
- 🦘 **Double Jump**: Nhảy 2 lần trong không trung
- 🧲 **Magnet Effect**: Hút coin từ xa
- 🛡️ **Shield**: Bảo vệ khỏi chướng ngại vật
- ⚡ **Speed Boost**: Tăng tốc độ di chuyển

## 🎮 Screenshots & Media

> **Gợi ý ảnh bạn nên thêm:**

### 1. **Logo/Banner Game** 
- Ảnh logo chính của game
- Banner với tên "Animal Rush" và artwork

### 2. **Main Menu**
- Screenshot màn hình menu chính
- Hiển thị UI design và các nút chức năng

### 3. **Character Selection**
- Ảnh màn hình chọn nhân vật
- Showcase các con vật khác nhau
- UI hiển thị stats và abilities

### 4. **Gameplay Screenshots**
- Ảnh trong game với player đang chạy
- Hiển thị obstacles, coins, enemies
- UI gameplay (coins, lives, score)

### 5. **Game Over Screen**
- Màn hình kết thúc với điểm số
- Leaderboard display

### 6. **GIF Animated**
- GIF ngắn showing gameplay
- Character abilities demonstration
- Obstacle interactions

*Example structure:*
```
![Main Menu](screenshots/main-menu.png)
![Character Selection](screenshots/character-select.png)
![Gameplay](screenshots/gameplay.gif)
```

## 🛠️ Phát Triển

### Công Nghệ Sử Dụng
- **Engine**: Unity 2022.3+
- **Language**: C#
- **Platform**: PC, Mobile Ready
- **Graphics**: 2D Sprites

### Cấu Trúc Dự Án
```
Assets/
├── Scripts/           # Game logic và controllers
├── Sprites/          # Art assets và animations
├── Scenes/           # Game scenes (MainMenu, MainLevel)
├── Prefabs/          # Prefabricated objects
├── Animation/        # Animation clips
├── Sounds/           # Audio files
└── Settings/         # Game settings và data
```

### Core Systems
- **GameManager**: Quản lý trạng thái game
- **PlayerController**: Điều khiển nhân vật
- **ObstacleSpawner**: Sinh chướng ngại vật
- **CoinController**: Hệ thống coin
- **PlayerManager**: Quản lý nhân vật và unlocks
- **AudioManager**: Quản lý âm thanh

## 🚀 Cài Đặt & Chạy Game

### Yêu Cầu Hệ Thống
- Unity 2022.3 hoặc mới hơn
- Visual Studio hoặc IDE tương tự
- Git (để clone repository)

### Hướng Dẫn Build
1. Clone repository:
   ```bash
   git clone https://github.com/luuconghoangnam/game-AnimalRush.git
   ```

2. Mở project trong Unity Hub

3. Chọn Unity version 2022.3+

4. Build cho platform mong muốn:
   - **PC**: File → Build Settings → PC, Mac & Linux Standalone
   - **Mobile**: File → Build Settings → Android/iOS

## 🎨 Art Style & Design

- **Style**: 2D Cartoon/Cute
- **Color Palette**: Bright và colorful
- **Characters**: Animal-themed với design đáng yêu
- **Environment**: Dynamic scrolling background

## 🔧 Cấu Hình & Tuning

Game có các settings có thể điều chỉnh:
- Obstacle spawn rates
- Coin generation
- Character stats
- Difficulty progression
- Audio levels

## 📈 Roadmap & Features

### ✅ Hoàn Thành
- [x] Core gameplay mechanics
- [x] Character system với multiple animals
- [x] Obstacle và enemy spawning
- [x] Coin collection system
- [x] Basic UI và menus
- [x] Audio integration

### 🔄 Đang Phát Triển
- [ ] More character animations
- [ ] Additional obstacle types
- [ ] Power-up system enhancement
- [ ] Mobile controls optimization

### 📋 Planned Features
- [ ] Multiple environments/themes
- [ ] Boss battles
- [ ] Daily challenges
- [ ] Social features (friend leaderboards)
- [ ] Achievement system
- [ ] Shop system với cosmetics

## 🤝 Đóng Góp

Mọi đóng góp đều được chào đón! Nếu bạn muốn:
- Report bugs
- Suggest features
- Submit art assets
- Code improvements

Hãy tạo Issue hoặc Pull Request.

## 📄 License

Dự án này được phát hành dưới [MIT License](LICENSE).

## 👨‍💻 Tác Giả

**Luu Cong Hoang Nam**
- GitHub: [@luuconghoangnam](https://github.com/luuconghoangnam)

## 🎉 Acknowledgments

- Unity Technologies for the game engine
- Asset artists and sound designers
- Beta testers và community feedback

---

### 🎮 Chơi Ngay!

*Download và trải nghiệm Animal Rush - cuộc phiêu lưu endless running đầy thú vị đang chờ bạn!*

[![Download](https://img.shields.io/badge/Download-Latest%20Release-brightgreen.svg)](https://github.com/luuconghoangnam/game-AnimalRush/releases)
