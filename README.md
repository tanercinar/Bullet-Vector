# Bullet Vector

Unity ile geliştirilmiş fizik tabanlı mekaniklere sahip aksiyon dolu bir FPS oyunu.

## Oyun

Oyunu tarayıcı üzerinden oynamak için tıklayın:
[https://tanercinar.itch.io/bullet-vector](https://tanercinar.itch.io/bullet-vector)

## Proje Hakkında

Bu proje Unity Oyun Motoru ile geliştirilmiştir. Ana karakter fizik tabanlı olarak hareket eder ve ona verilen kısıtlı alanda düşmanlardan kaçarak hayatta kalmaya çalışır.

## Kontroller

| Tuş | Eylem | Açıklama |
| :--- | :--- | :--- |
| **W, A, S, D** | Hareket | Karakterin x ve z ekseninde hareket etmesini sağlar. |
| **Mouse** | Bakış | Karakterin kafa ve vücut yönünü çevirir. |
| **Sol Tık** | Ateş Etme | Silahı ateşler ve düşmana hasar verir. |
| **Sağ Tık** | Dash (Atılma) | Karakterin hızlıca ileri atılmasını sağlar. |
| **Q** | Zamanı Yavaşlat | Zamanı geçici olarak yavaşlatır. |
| **Space** | Zıplama | Karakterin zıplamasını sağlar. |


## Oyuncu Mekanikleri

**1- Hareket etme**
Oyuncu zemin üzerinde hareket edebilir.

**2- Zıplama**
Oyuncu fizik kurallarına uygun şekilde zıplayabilir.

**3- Ateş Etme**
Oyuncu düşmanlarını vurmak için sılahını ateşleyebilir.

**3- Atılma (Dash)**
Oyuncu ani bir hızlanma ile hareket ettiği yöne doğru atılır.

**4- Zamanı Yavaşlatma**
Oyuncu kısa bir süreliğine zamanın akışını yavaşlatır ancak kendisi bundan etkilenmez.

**5-Hız ve Zıplama Arttırma**
Oyuncu topladığı güçlendirmelerle kısa süreliğine hızını ve zıplama gücünü iki katına çıkarır.

**6- Kalkan Güçlendirmesi**
Oyuncu topladığı kalkan güçlendirmesiyle alacağı bir sonraki hasarı engeller.

**7- Kalıcı Hasar Güçlendirmesi**
Oyuncu topladığı hasar güçlendirmesiyle düşmanlara verdiği hasarı kalıcı olark arttırır.


## Düşman Mekanikleri

**1- Ateş Etme**
Düşman oyuncuya zarar vermek için belirli aralıklarla mermi ateşleyebilir.

**2- Zıplama**
Düşman fizik kurallarına uygun şekilde zıplama hareketi yapabilir.

**3- Çarpışma Hasarı**
Düşman oyuncu ile fiziksel temas kurduğunda oyuncuya hasar verir ve kendini yok eder.

**4- Atılma (Dash)**
Düşman ani bir hızlanma ile bir yöne doğru atılarak oyuncuya yaklaşabilir veya saldırıdan kaçabilir.

**5- Hız ve Zıplama Arttırma**
Düşman sahnedeki güçlendirmeleri toplayarak kısa süreliğine hareket hızını ve zıplama gücünü iki katına çıkarır.

**6- Kalkan Güçlendirmesi**
Düşman kalkan güçlendirmesi topladığında görseli değişir ve alacağı bir sonraki hasarı tamamen engeller.

**7- Kalıcı Hasar Güçlendirmesi**
Düşman hasar güçlendirmesi toplayarak oyuncuya verdiği hasar miktarını kalıcı olarak arttırır.

## Grafik

Oyun motoru içindeki üç boyutlu nesnelerle basit grafikler oluşturulmuştur.

## Ses

* **Arka Plan Sesi:** Oyun boyunca atmosferi destekleyen döngüsel müzik.
* **Ateş Sesi:** Her atışta çalan ve silahın gücünü hissettiren ses efekti.

## Yapay Zeka

Bu projede düşmanlar, Q-Learning algoritması kullanılarak eğitilmektedir.

*   **Öğrenme Süreci:** Düşmanlar başlangıçta tamamen rastgele aksiyonlar alarak çevrelerini keşfederler. Zamanla, ödül (oyuncuyu vurma, öldürme) ve ceza (boşa ateş etme, hasar alma) mekanizmaları sayesinde en etkili stratejileri öğrenirler.
*   **Agresiflik ve Overfit:** Eğitim sonucunda düşmanların ateş etme  eylemine aşırı uyum sağladığı  ve son derece agresifleştiği gözlemlenmiştir. Eğitimini tamamlamış bir düşman, oyuncuyu gördüğü anda tereddüt etmeden ve yüksek sıklıkla ateş ederek baskı kurar. Öğrenmemiş düşmanlar rastgele hareket ederken, eğitilmiş düşmanlar ölümcül birer nişancıya dönüşür.
*   **Eksikler:** Düşmanlar ateş etmeyi iyi düzeyde öğrenmiş olsa da gereksiz zıplama ve atılma yapma gibi durumların önüne geçilememiştir, oyuncuyu ilk tespit etme noktasında düşmanlar yetersizdir.
