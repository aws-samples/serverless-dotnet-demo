const crypto = require('crypto');

const COLORS = [
  "Red", "Green", "Blue", "Yellow", "Orange", "Purple", "Pink", "Brown",
  "Black", "White", "Gray", "Silver", "Gold", "Cyan", "Magenta", "Maroon",
  "Navy", "Olive", "Teal", "Aqua", "Lime", "Coral", "Aquamarine",
  "Turquoise", "Violet", "Indigo", "Plum", "Crimson", "Salmon", "Coral",
  "Khaki", "Beige",
];

const PRODUCTS = [
  "Shoes", "Sweatshirts", "Hats", "Pants", "Shirts", "T-Shirts", "Trousers",
  "Jackets", "Shorts", "Skirts", "Dresses", "Coats", "Jeans", "Blazers",
  "Socks", "Gloves", "Belts", "Bags", "Shoes", "Sunglasses", "Watches",
  "Jewelry", "Ties", "Hair Accessories", "Makeup", "Accessories",
];

module.exports = {
  generateProduct: function (context, events, done) {
    const color = COLORS[Math.floor(Math.random() * COLORS.length)];
    const name = PRODUCTS[Math.floor(Math.random() * PRODUCTS.length)];

    context.vars.Id = crypto.randomUUID();
    context.vars.Name = `${color} ${name}`;
    context.vars.Price = Math.round(Math.random() * 10000) / 100;

    return done();
  },
};